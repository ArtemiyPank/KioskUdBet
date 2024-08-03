using BCrypt.Net;
using KioskAPI.Models;
using KioskAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace KioskAPI.Controllers
{
    public class RefreshTokenRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class AuthenticateRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Language { get; set; }
        public string Building { get; set; }
        public string RoomNumber { get; set; }
        public string Role { get; set; }
    }

    public class RegistrateRequest
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Language { get; set; }
        public string Building { get; set; }
        public string RoomNumber { get; set; }
        public string Role { get; set; } = "User";
    }

    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsersController> _logger;
        private readonly ITokenService _tokenService;

        public UsersController(IUserService userService, ITokenService tokenService, IConfiguration configuration, ILogger<UsersController> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        // Register a new user
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegistrateRequest request)
        {
            _logger.LogInformation($"request.Email - {request.Email}");
            _logger.LogInformation($"(await _userService.EmailExists(request.Email) - {await _userService.EmailExists(request.Email)}");

            if (await _userService.EmailExists(request.Email))
            {
                return BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Email already exists."
                });
            }

            var user = new User
            {
                Id = request.Id,
                Email = request.Email,
                Salt = BCrypt.Net.BCrypt.GenerateSalt(), // Generate salt
                FirstName = request.FirstName,
                LastName = request.LastName,
                Language = request.Language,
                Building = request.Building,
                RoomNumber = request.RoomNumber,
            };

            // Hash the password with the salt
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password + user.Salt);

            var newUser = await _userService.Register(user);

            var token = _tokenService.GenerateAccessToken(newUser);
            var refreshToken = _tokenService.GenerateRefreshToken(newUser);
            await _userService.SaveRefreshToken(refreshToken);

            var userResponse = new UserResponse
            {
                Id = newUser.Id,
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Language = newUser.Language,
                Building = newUser.Building,
                RoomNumber = newUser.RoomNumber,
                Role = newUser.Role
            };

            Response.Headers.Add("Access-Token", token);
            Response.Headers.Add("Refresh-Token", refreshToken.Token);

            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Message = "Registration successful",
                Data = userResponse
            });
        }

        // Authenticate a user with email and password
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
        {
            var user = await _userService.GetUserByEmail(request.Email);
            if (user == null) return Unauthorized(new ApiResponse
            {
                IsSuccess = false,
                Message = "Invalid email or password."
            });

            // Verify the password
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password + user.Salt, user.PasswordHash);
            if (!isPasswordValid) return Unauthorized(new ApiResponse
            {
                IsSuccess = false,
                Message = "Invalid email or password."
            });

            var token = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user);
            await _userService.SaveRefreshToken(refreshToken);

            var userResponse = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Language = user.Language,
                Building = user.Building,
                RoomNumber = user.RoomNumber,
                Role = user.Role
            };

            Response.Headers.Append("Access-Token", token);
            Response.Headers.Append("Refresh-Token", refreshToken.Token);

            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Message = "Authentication successful",
                Data = userResponse
            });
        }

        // Authenticate a user with a refresh token
        [HttpPost("authenticateWithToken")]
        public async Task<IActionResult> AuthenticateWithToken()
        {
            _logger.LogInformation("Received request for token authentication");
            try
            {
                // Get the refresh token from the header
                var refreshToken = Request.Headers["Refresh-Token"].ToString();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Refresh token not provided in headers");
                    return BadRequest(new ApiResponse
                    {
                        IsSuccess = false,
                        Message = "Refresh token not provided"
                    });
                }

                // Authenticate with the refresh token
                var response = await _userService.AuthenticateWithRefreshToken(refreshToken);

                if (response == null)
                {
                    _logger.LogWarning("User not found for the provided refresh token");
                    return Unauthorized(new ApiResponse
                    {
                        IsSuccess = false,
                        Message = "Invalid refresh token"
                    });
                }

                var (user, newJwtToken, newRefreshToken) = response.Value;

                var userResponse = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Language = user.Language,
                    Building = user.Building,
                    RoomNumber = user.RoomNumber,
                    Role = user.Role
                };

                Response.Headers.Append("Access-Token", newJwtToken);
                Response.Headers.Append("Refresh-Token", newRefreshToken);

                _logger.LogInformation($"User {user.Email} authenticated successfully");
                return Ok(new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Token authentication successful",
                    Data = userResponse
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AuthenticateWithToken method");
                return StatusCode(500, new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Internal server error"
                });
            }
        }

        // Refresh access and refresh tokens
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            // Проверяем валидность Refresh токена
            var user = await _userService.GetUserByRefreshToken(request.RefreshToken);
            if (user == null)
            {
                return Unauthorized(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Invalid refresh token"
                });
            }

            var newJwtToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user);

            await _userService.RevokeRefreshToken(user, request.RefreshToken);
            await _userService.SaveRefreshToken(newRefreshToken);

            Response.Headers.Append("Access-Token", newJwtToken);
            Response.Headers.Append("Refresh-Token", newRefreshToken.Token);

            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Message = "Token refreshed successfully",
                Data = user
            });
        }
    }
}

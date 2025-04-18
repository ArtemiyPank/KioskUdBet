using KioskAPI.Models;
using KioskAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace KioskAPI.Controllers
{
    // Request DTO for token refresh
    public class RefreshTokenRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    // Request DTO for user authentication
    public class AuthenticateRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Response DTO for user data
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
        public string? PlaceOfBirth { get; set; }
    }

    // Request DTO for registration
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
        public string? PlaceOfBirth { get; set; }
        public string Role { get; set; } = "User";
    }

    // Generic API response wrapper
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
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            ITokenService tokenService,
            IConfiguration configuration,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrateRequest request)
        {
            _logger.LogInformation("Registering user with email {Email}", request.Email);

            if (await _userService.EmailExistsAsync(request.Email))
            {
                _logger.LogWarning("Email {Email} already exists", request.Email);
                return BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Email already exists"
                });
            }

            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var user = new User
            {
                Email = request.Email,
                Salt = salt,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Language = request.Language,
                Building = request.Building,
                RoomNumber = request.RoomNumber,
                PlaceOfBirth = request.PlaceOfBirth
            };

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password + user.Salt);

            var newUser = await _userService.RegisterAsync(user);

            var accessToken = _tokenService.GenerateAccessToken(newUser);
            var refreshToken = _tokenService.GenerateRefreshToken(newUser);
            await _userService.SaveRefreshTokenAsync(newUser.Id, refreshToken.Token, refreshToken.ExpiryDate);

            Response.Headers.Append("Access-Token", accessToken);
            Response.Headers.Append("Refresh-Token", refreshToken.Token);

            var userResponse = new UserResponse
            {
                Id = newUser.Id,
                Email = newUser.Email,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Language = newUser.Language,
                Building = newUser.Building,
                RoomNumber = newUser.RoomNumber,
                Role = newUser.Role,
                PlaceOfBirth = newUser.PlaceOfBirth
            };

            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Message = "Registration successful",
                Data = userResponse
            });
        }

        // POST: api/users/authenticate
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
        {
            _logger.LogInformation("Authenticating user {Email}", request.Email);

            var user = await _userService.GetUserByEmailAsync(request.Email);

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password + user.Salt, user.PasswordHash);

            if (user == null || !isPasswordValid)
            {
                _logger.LogWarning("Invalid credentials for {Email}", request.Email);
                return Unauthorized(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Invalid email or password"
                });
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user);
            await _userService.SaveRefreshTokenAsync(user.Id, refreshToken.Token, refreshToken.ExpiryDate);

            Response.Headers.Append("Access-Token", accessToken);
            Response.Headers.Append("Refresh-Token", refreshToken.Token);

            var userResponse = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Language = user.Language,
                Building = user.Building,
                RoomNumber = user.RoomNumber,
                Role = user.Role,
                PlaceOfBirth = user.PlaceOfBirth
            };

            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Message = "Authentication successful",
                Data = userResponse
            });
        }

        // POST: api/users/authenticateWithToken
        [HttpPost("authenticateWithToken")]
        public async Task<IActionResult> AuthenticateWithToken()
        {
            _logger.LogInformation("Authenticating with refresh token");

            var refreshToken = Request.Headers["Refresh-Token"].ToString();
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh-Token header missing");
                return BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Refresh token not provided"
                });
            }

            var result = await _userService.AuthenticateWithRefreshTokenAsync(refreshToken);
            if (result == null)
            {
                _logger.LogWarning("Invalid refresh token");
                return Unauthorized(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Invalid refresh token"
                });
            }

            var (user, newAccessToken, newRefreshToken) = result.Value;

            Response.Headers.Append("Access-Token", newAccessToken);
            Response.Headers.Append("Refresh-Token", newRefreshToken);

            _logger.LogInformation("Refresh token authentication successful for {Email}", user.Email);

            var userResponse = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Language = user.Language,
                Building = user.Building,
                RoomNumber = user.RoomNumber,
                Role = user.Role,
                PlaceOfBirth = user.PlaceOfBirth
            };

            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Message = "Token authentication successful",
                Data = userResponse
            });
        }

        // POST: api/users/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            _logger.LogInformation("Refreshing tokens");

            var user = await _userService.GetUserByRefreshTokenAsync(request.RefreshToken);
            if (user == null)
            {
                _logger.LogWarning("Invalid refresh token in body");
                return Unauthorized(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Invalid refresh token"
                });
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user);
            await _userService.SaveRefreshTokenAsync(user.Id, newRefreshToken.Token, newRefreshToken.ExpiryDate);

            Response.Headers.Append("Access-Token", newAccessToken);
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

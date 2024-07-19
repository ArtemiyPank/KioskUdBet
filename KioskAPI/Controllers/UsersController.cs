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


        [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, IConfiguration configuration, ILogger<UsersController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegistrateRequest request)
        {
            if (await _userService.EmailExists(request.Email))
            {
                return BadRequest("Email already exists.");
            }

            var user = new User
            {
                Id = request.Id,
                Email = request.Email,
                Salt = BCrypt.Net.BCrypt.GenerateSalt(), // генерация соли
                FirstName = request.FirstName,
                LastName = request.LastName,
                Language = request.Language,
                Building = request.Building,
                RoomNumber = request.RoomNumber,
            };

            // Хеширование пароля с солью
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password + user.Salt);

            var newUser = await _userService.Register(user);
            var token = GenerateJwtToken(newUser);

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

            return Ok(new { user = userResponse, token });
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
        {
            var user = await _userService.GetUserByEmail(request.Email);
            if (user == null) return Unauthorized();

            // Проверка пароля
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password + user.Salt, user.PasswordHash);
            if (!isPasswordValid) return Unauthorized();

            var token = GenerateJwtToken(user);

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

            return Ok(new { user = userResponse, token });
        }

        [HttpGet("authenticateWithToken")]
        public async Task<IActionResult> AuthenticateWithToken()
        {
            _logger.LogInformation("Received request for token authentication");
            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                _logger.LogInformation($"Authorization header: {authHeader}");

                var token = authHeader.Replace("Bearer ", "");
                _logger.LogInformation($"Token: {token}");

                var user = await _userService.GetUserByToken(token);

                if (user == null)
                {
                    _logger.LogWarning("User not found for the provided token");
                    return Unauthorized();
                }

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

                _logger.LogInformation($"User {user.Email} authenticated successfully");
                return Ok(new { user = userResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AuthenticateWithToken method");
                return StatusCode(500, "Internal server error");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

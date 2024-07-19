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
        public async Task<IActionResult> Register(User user)
        {
            if (await _userService.EmailExists(user.Email))
            {
                return BadRequest("Email already exists.");
            }
            var newUser = await _userService.Register(user);
            var token = GenerateJwtToken(newUser);
            return Ok(new { user = newUser, token });
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
        {
            var user = await _userService.Authenticate(request.Email, request.Password);
            _logger.LogInformation(request.Email, request.Password);
            if (user == null) return Unauthorized();
            var token = GenerateJwtToken(user);

            //_logger.LogInformation(user.ToString(), token);

            return Ok(new { user, token });
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

                _logger.LogInformation($"User {user.Email} authenticated successfully");
                return Ok(new { user });
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
            new Claim(JwtRegisteredClaimNames.Email, user.Email.ToString()),
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

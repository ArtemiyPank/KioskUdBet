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
            var newUser = await _userService.Register(user);
            var token = GenerateJwtToken(newUser);
            return Ok(new { user = newUser, token });
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User login)
        {
            var user = await _userService.Authenticate(login.Username, login.Password);
            if (user == null) return Unauthorized();
            var token = GenerateJwtToken(user);

            //_logger.LogInformation(user.ToString(), token);

            return Ok(new { user, token });
        }

        [HttpGet("authenticateWithToken")]
        public async Task<IActionResult> AuthenticateWithToken()
        {
            _logger.LogInformation("Получен запрос на аутентификацию по токену");
            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                _logger.LogInformation($"Заголовок авторизации: {authHeader}");

                var token = authHeader.Replace("Bearer ", "");
                _logger.LogInformation($"Токен: {token}");

                var user = await _userService.GetUserByToken(token);

                if (user == null)
                {
                    _logger.LogWarning("Пользователь не найден по предоставленному токену");
                    return Unauthorized();
                }

                _logger.LogInformation($"Пользователь {user.Username} аутентифицирован успешно");
                return Ok(new { user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в методе AuthenticateWithToken");
                return StatusCode(500, "Внутренняя ошибка сервера");
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
                    new Claim(ClaimTypes.Name, user.Username.ToString()),
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

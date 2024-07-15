using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly ApplicationDbContext _context;

        public UserService(ILogger<UserService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            // Ищем пользователя по username и password
            return await _context.Users.SingleOrDefaultAsync(x => x.Username == username && x.Password == password);
        }

        public async Task<User> GetUserByToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token is null or empty");
                return null;
            }

            _logger.LogInformation("Processing token");

            JwtSecurityToken jwtToken;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read token");
                return null;
            }

            if (jwtToken == null)
            {
                _logger.LogWarning("Invalid JWT token");
                return null;
            }

            var username = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.UniqueName)?.Value;

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Username not found in token");
                return null;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                _logger.LogWarning("User not found with username: {Username}", username);
            }
            else
            {
                _logger.LogInformation("User authenticated: {Username}", user.Username);
            }

            return user;
        }



        public async Task<User> Register(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }
    }
}

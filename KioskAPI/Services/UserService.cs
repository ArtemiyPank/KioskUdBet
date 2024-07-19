using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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


        public async Task<User> Authenticate(string email, string password)
        {
            // Ищем пользователя по username и password
            return await _context.Users.SingleOrDefaultAsync(x => x.Email == email && x.Password == password);
        }


        public async Task<User> GetUserByToken(string token)
        {
            _logger.LogInformation("I am in GetUserByToken!");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
            {
                _logger.LogWarning("Invalid JWT token");
                return null;
            }

            foreach (var claim in jwtToken.Claims)
            {
                _logger.LogInformation($"Claim type: {claim.Type}, value: {claim.Value}");
            }

            var email = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            if (email == null)
            {
                email = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;
            }

            if (email == null)
            {
                _logger.LogWarning("Email not found in token");
                return null;
            }

            _logger.LogInformation($"email: {email}");

            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }





        public async Task<User> Register(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }


        public async Task<bool> EmailExists(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }


        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }
    }
}

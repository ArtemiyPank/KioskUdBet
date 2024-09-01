using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskAPI.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public UserService(ILogger<UserService> logger, ApplicationDbContext context, ITokenService tokenService)
        {
            _logger = logger;
            _context = context;
            _tokenService = tokenService;
        }

        // Authenticate user by email and password
        public async Task<User?> Authenticate(string email, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == email);
            if (user == null) return null;

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password + user.Salt, user.PasswordHash);
            return isPasswordValid ? user : null;
        }

        // Authenticate user using refresh token
        public async Task<(User, string, string)?> AuthenticateWithRefreshToken(string refreshToken)
        {
            var user = await GetUserByRefreshToken(refreshToken);

            if (user == null)
            {
                return null;
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user);

            // Обновление существующего токена
            await SaveRefreshToken(user.Id, newRefreshToken.Token, newRefreshToken.ExpiryDate);

            return (user, newAccessToken, newRefreshToken.Token);
        }

        // Register a new user
        public async Task<User> Register(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // Check if email exists in the database
        public async Task<bool> EmailExists(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        // Get all users
        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // Get user by email
        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        // Get user by ID
        public async Task<User?> GetUserById(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        // Save refresh token in the database (обновляем или создаем новый)
        public async Task SaveRefreshToken(int userId, string token, DateTime expiryDate)
        {
            var refreshToken = await _context.RefreshTokens.SingleOrDefaultAsync(rt => rt.UserId == userId);

            if (refreshToken != null)
            {
                // Обновление существующего токена
                refreshToken.Token = token;
                refreshToken.ExpiryDate = expiryDate;
                refreshToken.IsRevoked = false;
            }
            else
            {
                // Создание нового токена
                refreshToken = new RefreshToken
                {
                    UserId = userId,
                    Token = token,
                    ExpiryDate = expiryDate,
                    Created = DateTime.UtcNow
                };
                _context.RefreshTokens.Add(refreshToken);
            }

            await _context.SaveChangesAsync();
        }

        // Validate refresh token
        public async Task<bool> ValidateRefreshToken(User user, string refreshToken)
        {
            var token = await _context.RefreshTokens
                .SingleOrDefaultAsync(t => t.Token == refreshToken && t.UserId == user.Id && !t.IsRevoked);
            return token != null && token.ExpiryDate > DateTime.UtcNow;
        }

        // Revoke refresh token
        public async Task RevokeRefreshToken(User user, string refreshToken)
        {
            var token = await _context.RefreshTokens
                .SingleOrDefaultAsync(t => t.Token == refreshToken && t.UserId == user.Id);
            if (token != null)
            {
                token.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }

        // Get user by refresh token
        public async Task<User?> GetUserByRefreshToken(string refreshToken)
        {
            var token = await _context.RefreshTokens.Include(rt => rt.User)
                                                     .SingleOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            return token?.User;
        }
    }

}


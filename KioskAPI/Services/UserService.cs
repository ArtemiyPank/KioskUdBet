using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public UserService(
            ILogger<UserService> logger,
            ApplicationDbContext context,
            ITokenService tokenService)
        {
            _logger = logger;
            _context = context;
            _tokenService = tokenService;
        }

        // Authenticate a user by email and password
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            _logger.LogInformation("Authenticating user with email {Email}", email);
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                _logger.LogWarning("User with email {Email} not found", email);
                return null;
            }

            var isValid = BCrypt.Net.BCrypt.Verify(password + user.Salt, user.PasswordHash);
            if (!isValid)
            {
                _logger.LogWarning("Invalid password for user {Email}", email);
                return null;
            }

            _logger.LogInformation("User {Email} authenticated successfully", email);
            return user;
        }

        // Authenticate using a refresh token and return new tokens
        public async Task<(User, string, string)?> AuthenticateWithRefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Authenticating with refresh token");
            var user = await GetUserByRefreshTokenAsync(refreshToken);
            if (user == null)
            {
                _logger.LogWarning("Invalid refresh token");
                return null;
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user).Token;
            await SaveRefreshTokenAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddYears(1));

            _logger.LogInformation("Refresh token authentication successful for user {Email}", user.Email);
            return (user, accessToken, newRefreshToken);
        }

        // Register a new user
        public async Task<User> RegisterAsync(User user)
        {
            _logger.LogInformation("Registering user with email {Email}", user.Email);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {Email} registered with ID {UserId}", user.Email, user.Id);
            return user;
        }

        // Check if an email is already registered
        public async Task<bool> EmailExistsAsync(string email)
        {
            _logger.LogInformation("Checking if email {Email} exists", email);
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        // Retrieve all users
        public async Task<List<User>> GetAllUsersAsync()
        {
            _logger.LogInformation("Retrieving all users");
            return await _context.Users.ToListAsync();
        }

        // Retrieve a user by email
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            _logger.LogInformation("Fetching user by email {Email}", email);
            return await _context.Users
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        // Retrieve a user by ID
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            _logger.LogInformation("Fetching user by ID {UserId}", userId);
            return await _context.Users.FindAsync(userId);
        }

        // Save or update a refresh token for a user
        public async Task SaveRefreshTokenAsync(int userId, string token, DateTime expiryDate)
        {
            _logger.LogInformation("Saving refresh token for user {UserId}", userId);

            var existing = await _context.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.UserId == userId);

            if (existing != null)
            {
                existing.Token = token;
                existing.ExpiryDate = expiryDate;
                existing.IsRevoked = false;
            }
            else
            {
                var refreshToken = new RefreshToken
                {
                    UserId = userId,
                    Token = token,
                    ExpiryDate = expiryDate,
                    Created = DateTime.UtcNow
                };
                _context.RefreshTokens.Add(refreshToken);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Refresh token saved for user {UserId}", userId);
        }

        // Validate that the refresh token belongs to the user and is active
        public async Task<bool> ValidateRefreshTokenAsync(User user, string refreshToken)
        {
            _logger.LogInformation("Validating refresh token for user {UserId}", user.Id);

            var token = await _context.RefreshTokens
                .SingleOrDefaultAsync(rt =>
                    rt.UserId == user.Id &&
                    rt.Token == refreshToken &&
                    !rt.IsRevoked);

            var isValid = token != null && token.ExpiryDate > DateTime.UtcNow;
            _logger.LogInformation(
                "Refresh token validation for user {UserId} returned {IsValid}",
                user.Id, isValid);

            return isValid;
        }

        // Revoke a given refresh token for the user
        public async Task RevokeRefreshTokenAsync(User user, string refreshToken)
        {
            _logger.LogInformation("Revoking refresh token for user {UserId}", user.Id);

            var token = await _context.RefreshTokens
                .SingleOrDefaultAsync(rt =>
                    rt.UserId == user.Id &&
                    rt.Token == refreshToken);

            if (token != null)
            {
                token.IsRevoked = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Refresh token revoked for user {UserId}", user.Id);
            }
        }

        // Retrieve the user associated with a given refresh token
        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Fetching user by refresh token");

            var token = await _context.RefreshTokens
                .Include(rt => rt.User)
                .SingleOrDefaultAsync(rt =>
                    rt.Token == refreshToken &&
                    !rt.IsRevoked);

            return token?.User;
        }
    }
}

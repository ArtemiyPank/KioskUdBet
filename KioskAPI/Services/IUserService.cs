using KioskAPI.Models;

namespace KioskAPI.Services
{
    public interface IUserService
    {
        // Authenticate a user by email and password
        Task<User?> AuthenticateAsync(string email, string password);

        // Authenticate using a refresh token and return new tokens
        Task<(User user, string newAccessToken, string newRefreshToken)?> AuthenticateWithRefreshTokenAsync(string refreshToken);

        // Register a new user
        Task<User> RegisterAsync(User user);

        // Retrieve all users
        Task<List<User>> GetAllUsersAsync();

        // Check if an email is already registered
        Task<bool> EmailExistsAsync(string email);

        // Retrieve a user by email
        Task<User?> GetUserByEmailAsync(string email);

        // Save a refresh token for the specified user
        Task SaveRefreshTokenAsync(int userId, string token, DateTime expiryDate);

        // Validate that a refresh token belongs to the user
        Task<bool> ValidateRefreshTokenAsync(User user, string refreshToken);

        // Revoke a refresh token for the user
        Task RevokeRefreshTokenAsync(User user, string refreshToken);

        // Retrieve the user associated with a given refresh token
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);

        // Retrieve a user by their identifier
        Task<User> GetUserByIdAsync(int userId);
    }
}

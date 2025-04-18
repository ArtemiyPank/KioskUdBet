using KioskApp.Models;

namespace KioskApp.Services
{
    public interface IUserService
    {
        // Register a new user and return API response
        Task<ApiResponse> RegisterAsync(User user);

        // Authenticate with email and password
        Task<ApiResponse> AuthenticateAsync(string email, string password);

        // Authenticate using stored refresh token
        Task<User> AuthenticateWithTokenAsync();

        // Retrieve all registered users
        Task<List<User>> GetAllUsersAsync();

        // Get the user currently stored in app state
        User GetCurrentUser();

        // Set the current user in app state
        void SetCurrentUser(User user);

        // Clear the current user data from storage
        Task ClearCurrentUserAsync();

        // Log out the user and clear authentication tokens
        Task LogoutAsync();
    }
}

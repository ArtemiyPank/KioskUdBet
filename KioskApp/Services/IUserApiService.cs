using KioskApp.Models;

namespace KioskApp.Services
{
    public interface IUserApiService
    {
        // Send a generic HTTP request; set isSseRequest = true for Server-Sent Events
        Task<HttpResponseMessage> SendRequestAsync(
            Func<HttpRequestMessage> createRequest,
            bool isSseRequest = false);

        // Register a new user and return authentication response
        Task<AuthResponse> RegisterUserAsync(User user);

        // Authenticate user with email and password
        Task<AuthResponse> AuthenticateUserAsync(string email, string password);

        // Authenticate using a refresh token and return new tokens
        Task<AuthResponse> AuthenticateWithTokenAsync(string refreshToken);

        // Refresh the stored access and refresh tokens
        Task RefreshTokensAsync();
    }
}

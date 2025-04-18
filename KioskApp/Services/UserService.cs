using System.Diagnostics;
using KioskApp.Helpers;
using KioskApp.Models;

namespace KioskApp.Services
{
    public class UserService : IUserService
    {
        private readonly IUserApiService _userApiService;
        private readonly AppState _appState;

        public UserService()
        {
            _userApiService = DependencyService.Get<IUserApiService>();
            _appState = DependencyService.Get<AppState>();
        }

        // Register a new user and update application state
        public async Task<ApiResponse> RegisterAsync(User user)
        {
            var response = await _userApiService.RegisterUserAsync(user);
            if (response.IsSuccess)
            {
                await SetCurrentUserAsync(response.User, response.AccessToken, response.RefreshToken);
                Debug.WriteLine($"Registered user: {response.User.Email}");
            }
            else
            {
                Debug.WriteLine($"Registration failed: {response.Message}");
            }

            MessagingCenter.Send(this, "UserStateChanged");

            return new ApiResponse
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.User
            };
        }

        // Authenticate using email and password
        public async Task<ApiResponse> AuthenticateAsync(string email, string password)
        {
            var response = await _userApiService.AuthenticateUserAsync(email, password);
            if (response.IsSuccess)
            {
                await SetCurrentUserAsync(response.User, response.AccessToken, response.RefreshToken);
                Debug.WriteLine($"Authenticated user: {response.User.Email}");
            }
            else
            {
                Debug.WriteLine($"Authentication failed: {response.Message}");
            }

            MessagingCenter.Send(this, "UserStateChanged");

            return new ApiResponse
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.User
            };
        }

        // Authenticate with stored refresh token
        public async Task<User> AuthenticateWithTokenAsync()
        {
            try
            {
                var refreshToken = await SecureStorage.GetAsync("refresh_token");
                if (string.IsNullOrEmpty(refreshToken))
                {
                    Debug.WriteLine("No refresh token found.");
                    return null;
                }

                var response = await _userApiService.AuthenticateWithTokenAsync(refreshToken);
                if (response?.IsSuccess == true)
                {
                    await SetCurrentUserAsync(response.User, response.AccessToken, response.RefreshToken);
                    Debug.WriteLine($"Authenticated with token: {response.User.Email}");
                    MessagingCenter.Send(this, "UserStateChanged");
                    return response.User;
                }

                Debug.WriteLine($"Token authentication failed: {response?.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error authenticating with token: {ex.Message}");
                return null;
            }
        }

        // Log out the current user and clear stored tokens
        public async Task LogoutAsync()
        {
            await ClearCurrentUserAsync();
            MessagingCenter.Send(this, "UserStateChanged");
        }

        // Get the currently logged in user from application state
        public User GetCurrentUser() => _appState.CurrentUser;

        // Retrieve all users (not implemented)
        public async Task<List<User>> GetAllUsersAsync()
        {
            return new List<User>();
        }

        // Set the current user in application state
        public void SetCurrentUser(User user)
        {
            _appState.CurrentUser = user;
            Debug.WriteLine($"Current user set: {user.Email}");
        }

        // Clear current user and tokens from state and secure storage
        public async Task ClearCurrentUserAsync()
        {
            _appState.CurrentUser = null;
            _appState.AccessToken = null;
            await SecureStorage.SetAsync("auth_token", string.Empty);
            await SecureStorage.SetAsync("refresh_token", string.Empty);
            Debug.WriteLine("User logged out and tokens cleared.");
        }

        // Store user and tokens in state and secure storage
        private async Task SetCurrentUserAsync(User user, string accessToken, string refreshToken)
        {
            _appState.CurrentUser = user;
            _appState.AccessToken = accessToken;
            await SecureStorage.SetAsync("auth_token", accessToken);
            await SecureStorage.SetAsync("refresh_token", refreshToken);
        }
    }
}

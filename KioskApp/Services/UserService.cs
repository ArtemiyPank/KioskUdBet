using KioskApp.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace KioskApp.Services
{
    public class UserService : IUserService
    {
        private User _currentUser;
        private string _accessToken;
        private string _refreshToken;

        // Register a new user and store the tokens
        public async Task<User> Register(User user)
        {
            var (registeredUser, token, refreshToken) = await DependencyService.Get<IApiService>().RegisterUser(user);
            if (registeredUser != null)
            {
                await SetCurrentUserAndTokens(registeredUser, token, refreshToken);
                Debug.WriteLine($"Registered User in UserService: {registeredUser.Email}");
            }
            else
            {
                Debug.WriteLine("Registration failed in UserService.");
            }
            return registeredUser;
        }

        // Authenticate a user using email and password, and store the tokens
        public async Task<User> Authenticate(string email, string password)
        {
            var (user, token, refreshToken) = await DependencyService.Get<IApiService>().AuthenticateUser(email, password);
            if (user != null)
            {
                await SetCurrentUserAndTokens(user, token, refreshToken);
                Debug.WriteLine($"Authenticated User in UserService: {user.Email}");
            }
            else
            {
                Debug.WriteLine("Authentication failed in UserService.");
            }
            return user;
        }

        // Authenticate a user using a refresh token
        public async Task<User> AuthenticateWithToken()
        {
            var refreshToken = await SecureStorage.GetAsync("refresh_token");

            var (user, newAccessToken, newRefreshToken) = await DependencyService.Get<IApiService>().AuthenticateWithToken(refreshToken);

            if (user != null)
            {
                await SetCurrentUserAndTokens(user, newAccessToken, newRefreshToken);
                Debug.WriteLine($"Authenticated User with token in UserService: {user.Email}");
            }
            else
            {
                Debug.WriteLine("Token authentication failed in UserService.");
            }
            return user;
        }

        // Get the current authenticated user
        public User GetCurrentUser()
        {
            return _currentUser;
        }

        // Temporarily return an empty list of users
        public async Task<List<User>> GetAllUsers()
        {
            return new List<User>();
        }

        // Set the current user
        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            Debug.WriteLine($"Set Current User in UserService: {_currentUser?.Email}");
        }

        // Clear the current user and tokens
        public async Task ClearCurrentUserAsync()
        {
            _currentUser = null;
            _accessToken = null;
            _refreshToken = null;
            await SecureStorage.SetAsync("auth_token", string.Empty);
            await SecureStorage.SetAsync("refresh_token", string.Empty);
            DependencyService.Get<IApiService>().ClearAuthorizationHeader();
            Debug.WriteLine("User has been logged out and tokens removed.");
        }

        // Helper method to set the current user and store tokens
        private async Task SetCurrentUserAndTokens(User user, string accessToken, string refreshToken)
        {
            _currentUser = user;
            _accessToken = accessToken;
            _refreshToken = refreshToken;

            await SecureStorage.SetAsync("auth_token", accessToken); // Store the access token
            await SecureStorage.SetAsync("refresh_token", refreshToken); // Store the refresh token
        }
    }
}

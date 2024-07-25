using KioskApp.Models;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public class UserService : IUserService
    {
        private User _currentUser;
        private string _accessToken;
        private string _refreshToken;

        public async Task<ApiResponse> Register(User user)
        {
            var response = await DependencyService.Get<IApiService>().RegisterUser(user);
            if (response.IsSuccess)
            {
                await SetCurrentUserAsync(response.User, response.AccessToken, response.RefreshToken);
                Debug.WriteLine($"Registered User in UserService: {response.User.Email}");
            }
            else
            {
                Debug.WriteLine($"Registration failed in UserService: {response.Message}");
            }

            ApiResponse completeResponse = new ApiResponse()
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.User
            };

            return completeResponse;
        }

        public async Task<ApiResponse> Authenticate(string email, string password)
        {
            var response = await DependencyService.Get<IApiService>().AuthenticateUser(email, password);
            if (response.IsSuccess)
            {
                await SetCurrentUserAsync(response.User, response.AccessToken, response.RefreshToken);
                Debug.WriteLine($"Authenticated User in UserService: {response.User.Email}");
            }
            else
            {
                Debug.WriteLine($"Authentication failed in UserService: {response.Message}");
            }

            ApiResponse completeResponse = new ApiResponse()
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.User
            };
            return completeResponse;
        }

        public async Task<User> AuthenticateWithToken()
        {
            var refreshToken = await SecureStorage.GetAsync("refresh_token");

            var response = await DependencyService.Get<IApiService>().AuthenticateWithToken(refreshToken);

            if (response.IsSuccess)
            {
                await SetCurrentUserAsync(response.User, response.AccessToken, response.RefreshToken);
                Debug.WriteLine($"Authenticated User with token in UserService: {response.User.Email}");
            }
            else
            {
                Debug.WriteLine($"Token authentication failed in UserService: {response.Message}");
            }


            // !!! it is necessary to register the logic of logging out of the account and error output
            return response.User;
        }

        public User GetCurrentUser()
        {
            return _currentUser;
        }

        public async Task<List<User>> GetAllUsers()
        {
            // Temporarily return an empty list
            return new List<User>();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            Debug.WriteLine($"Set Current User in UserService: {_currentUser?.Email}");
        }

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

        private async Task SetCurrentUserAsync(User user, string accessToken, string refreshToken)
        {
            _currentUser = user;
            _accessToken = accessToken;
            _refreshToken = refreshToken;

            await SecureStorage.SetAsync("auth_token", accessToken); // Store the new access token
            await SecureStorage.SetAsync("refresh_token", refreshToken); // Store the new refresh token
        }
    }
}

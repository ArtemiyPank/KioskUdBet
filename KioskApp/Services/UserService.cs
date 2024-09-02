using KioskApp.Helpers;
using KioskApp.Models;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public class UserService : IUserService
    {
        private IUserApiService _userApiService;
        private readonly AppState _appState;


        public UserService()
        {
            _userApiService = DependencyService.Get<IUserApiService>();
            _appState = DependencyService.Get<AppState>();
        }

        public async Task<ApiResponse> Register(User user)
        {
            var response = await _userApiService.RegisterUser(user);
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

            MessagingCenter.Send(this, "UserStateChanged"); // To update the main and product page

            return completeResponse;
        }

        public async Task<ApiResponse> Authenticate(string email, string password)
        {
            var response = await _userApiService.AuthenticateUser(email, password);
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

            MessagingCenter.Send(this, "UserStateChanged"); // To update the main and product page

            return completeResponse;
        }

        public async Task<User> AuthenticateWithToken()
        {
            try
            {
                var refreshToken = await SecureStorage.GetAsync("refresh_token");

                if (string.IsNullOrEmpty(refreshToken))
                {
                    Debug.WriteLine("No refresh token found.");
                    return null;
                }

                var response = await _userApiService.AuthenticateWithToken(refreshToken);
                Debug.WriteLine("AuthenticateWithToken 3");

                if (response?.IsSuccess == true)
                {
                    await SetCurrentUserAsync(response.User, response.AccessToken, response.RefreshToken);
                    Debug.WriteLine($"Authenticated User with token in UserService: {response.User.Email}");

                    MessagingCenter.Send(this, "UserStateChanged"); // To update the main and product page

                    return response.User;

                }
                else
                {
                    Debug.WriteLine($"Token authentication failed in UserService: {response?.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AuthenticateWithToken: {ex.Message}");
                return null;
            }


        }

        public async Task Logout()
        {
            await ClearCurrentUserAsync();

            MessagingCenter.Send(this, "UserStateChanged"); // To update the main and product page
        }

        public User GetCurrentUser()
        {
            return _appState.CurrentUser;
        }

        public async Task<List<User>> GetAllUsers()
        {
            // Temporarily return an empty list
            return new List<User>();
        }

        public void SetCurrentUser(User user)
        {
            _appState.CurrentUser = user;
            Debug.WriteLine($"Set Current User in UserService: {_appState.CurrentUser?.Email}");
        }

        public async Task ClearCurrentUserAsync()
        {
            _appState.CurrentUser = null;
            _appState.AccessToken = null;
            await SecureStorage.SetAsync("auth_token", string.Empty);
            await SecureStorage.SetAsync("refresh_token", string.Empty);
            Debug.WriteLine("User has been logged out and tokens removed.");
        }

        private async Task SetCurrentUserAsync(User user, string accessToken, string refreshToken)
        {
            _appState.CurrentUser = user;
            _appState.AccessToken = accessToken;

            await SecureStorage.SetAsync("auth_token", accessToken); // Store the new access token
            await SecureStorage.SetAsync("refresh_token", refreshToken); // Store the new refresh token
        }

    }
}

using KioskApp.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public class UserService : IUserService
    {
        private User _currentUser;


        public async Task<(User, string)> Register(User user)
        {
            var (registeredUser, token) = await DependencyService.Get<IApiService>().RegisterUser(user);
            if (registeredUser != null)
            {
                _currentUser = registeredUser;
                Debug.WriteLine($"Registered User in UserService: {registeredUser.Email}");
            }
            else
            {
                Debug.WriteLine("Registration failed in UserService.");
            }
            return (registeredUser, token);
        }

        public async Task<(User, string)> Authenticate(string username, string password)
        {
            var (user, token) = await DependencyService.Get<IApiService>().AuthenticateUser(username, password);
            if (user != null)
            {
                _currentUser = user;
                Debug.WriteLine($"Authenticated User in UserService: {user.Email}");

            }
            else
            {
                Debug.WriteLine("Authentication failed in UserService.");
            }
            return (user, token);
        }

        public async Task<User> AuthenticateWithToken(string token)
        {
            var user = await DependencyService.Get<IApiService>().AuthenticateWithToken(token);

            if (user != null)
            {
                _currentUser = user;
                Debug.WriteLine($"Authenticated User with token in UserService: {user.Email}");
            }
            else
            {
                Debug.WriteLine("Token authentication failed in UserService.");
            }
            return user;
        }

        public User GetCurrentUser()
        {
            return _currentUser;
        }

        public async Task<List<User>> GetAllUsers()
        {
            // Временно возвращаем пустой список
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
            await SecureStorage.SetAsync("auth_token", string.Empty);
            Debug.WriteLine("User has been logged out and token removed.");
        }
    }
}

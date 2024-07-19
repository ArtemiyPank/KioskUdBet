using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KioskApp.Services;
using KioskApp.Views;
using Microsoft.Maui.Controls;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IUserService _userService;

        public LoginViewModel()
        {
            _apiService = DependencyService.Get<IApiService>();
            _userService = DependencyService.Get<IUserService>();
        }


        public string Email { get; set; }
        public string Password { get; set; }

        public async Task<bool> Login()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                return false;
            }

            var (user, token) = await _userService.Authenticate(Email, Password);
            if (user != null)
            {
                _userService.SetCurrentUser(user);
                await SecureStorage.SetAsync("auth_token", token); // Сохранение токена
                Debug.WriteLine($"Authenticated User in LoginViewModel: {user.Email}");
                await Shell.Current.GoToAsync(".."); // Переход на предыдущую страницу(ProfilePage)
                MessagingCenter.Send(this, "UpdateUserState");

                return true;
            }
            else
            {
                // Login error
                // Дописать логику
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private async void OnNavigateToRegister()
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}

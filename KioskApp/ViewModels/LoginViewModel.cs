using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KioskApp.Services;
using KioskApp.Views;
using KioskApp.Models;
using MvvmHelpers;
using Microsoft.Maui.Controls;

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

        private string email;
        public string Email
        {
            get => email;
            set
            {
                SetProperty(ref email, value);
                OnPropertyChanged();
            }
        }

        private string password;
        public string Password
        {
            get => password;
            set
            {
                SetProperty(ref password, value);
                OnPropertyChanged();
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                SetProperty(ref errorMessage, value);
                OnPropertyChanged();
            }
        }

        public async Task<bool> Login()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Email and Password cannot be empty.";
                return false;
            }

            var authResponse = await _userService.Authenticate(Email, Password);
            if (authResponse != null && authResponse.IsSuccess)
            {
                Debug.WriteLine($"Authenticated User in LoginViewModel: {authResponse.Data.Email}");
                await Shell.Current.GoToAsync(".."); // Navigate to previous page (ProfilePage)

                MessagingCenter.Send(this, "UpdateUserState");

                return true;
            }
            else
            {
                ErrorMessage = authResponse?.Message ?? "Login failed.";
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

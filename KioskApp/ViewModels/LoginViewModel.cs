using System.Diagnostics;
using System.Windows.Input;
using KioskApp.Services;
using KioskApp.Views;
using Microsoft.Maui.Controls;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        public LoginViewModel()
        {
            _userService = DependencyService.Get<IUserService>();
            LoginCommand = new Command(OnLogin);
            NavigateToRegisterCommand = new Command(OnNavigateToRegister);
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public string ErrorMessage { get; set; }
        public bool IsErrorVisible { get; set; }

        private async void OnLogin()
        {
            var (user, token) = await _userService.Authenticate(Username, Password);
            if (user != null)
            {
                _userService.SetCurrentUser(user);
                await SecureStorage.SetAsync("auth_token", token); // Сохранение токена
                Debug.WriteLine($"Authenticated User in LoginViewModel: {user.Username}");
                await Shell.Current.GoToAsync(".."); // Переход на предыдущую страницу(ProfilePage)
                MessagingCenter.Send(this, "UpdateUserState");
            }
            else
            {
                ErrorMessage = "Invalid username or password";
                IsErrorVisible = true;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(IsErrorVisible));
            }
        }

        private async void OnNavigateToRegister()
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}

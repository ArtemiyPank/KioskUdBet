using System.Diagnostics;
using System.Windows.Input;
using KioskApp.Models;
using KioskApp.Services;
using Microsoft.Maui.Controls;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        public RegisterViewModel()
        {
            _userService = DependencyService.Get<IUserService>();
            RegisterCommand = new Command(OnRegister);
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public ICommand RegisterCommand { get; }
        public string ErrorMessage { get; set; }
        public bool IsErrorVisible { get; set; }

        private async void OnRegister()
        {
            var user = new User { Username = Username, Password = Password, Role = "User" };
            var (registeredUser, token) = await _userService.Register(user);
            if (registeredUser != null)
            {
                _userService.SetCurrentUser(registeredUser);
                await SecureStorage.SetAsync("auth_token", token); // Сохранение токена
                Debug.WriteLine($"Registered User in RegisterViewModel: {registeredUser.Username}");
                await Shell.Current.GoToAsync(".."); // Переход на предыдущую страницу, ProfilePage должна быть предшествующей страницей
                MessagingCenter.Send(this, "UpdateUserState");
            }
            else
            {
                ErrorMessage = "Registration failed";
                IsErrorVisible = true;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(IsErrorVisible));
            }
        }
    }
}

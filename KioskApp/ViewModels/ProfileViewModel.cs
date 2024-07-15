using System.Diagnostics;
using System.Windows.Input;
using KioskApp.Models;
using KioskApp.Services;
using KioskApp.Views;
using Microsoft.Maui.Controls;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        public ProfileViewModel()
        {
            _userService = DependencyService.Get<IUserService>();
            NavigateToLoginCommand = new Command(OnNavigateToLogin);
            NavigateToRegisterCommand = new Command(OnNavigateToRegister);
            LogoutCommand = new Command(OnLogout);

            UpdateUserState();

            MessagingCenter.Subscribe<App>(this, "UpdateUserState", (sender) =>
            {
                UpdateUserState();
            });
        }

        public User CurrentUser => _userService.GetCurrentUser();
        public bool IsAuthenticated => CurrentUser != null;
        public bool IsNotAuthenticated => !IsAuthenticated;

        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand LogoutCommand { get; }

        private async void OnNavigateToLogin()
        {
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }

        private async void OnNavigateToRegister()
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }

        private async void OnLogout()
        {
            await _userService.ClearCurrentUserAsync();
            UpdateUserState();
        }

        public void UpdateUserState()
        {
            var currentUser = _userService.GetCurrentUser();
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(IsNotAuthenticated));
        }
    }
}

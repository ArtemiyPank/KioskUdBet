using System.Diagnostics;
using System.Windows.Input;
using KioskApp.Models;
using KioskApp.Services;
using KioskApp.Views;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        // Commands for navigation and logout
        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand LogoutCommand { get; }

        public ProfileViewModel()
        {
            try
            {
                _userService = DependencyService.Get<IUserService>();
                NavigateToLoginCommand = new Command(OnNavigateToLogin);
                NavigateToRegisterCommand = new Command(OnNavigateToRegister);
                LogoutCommand = new Command(OnLogout);

                // Initialize UI state based on current user
                UpdateUserState();

                // Refresh UI when user state changes elsewhere
                MessagingCenter.Subscribe<App>(this, "UserStateChanged", sender =>
                {
                    UpdateUserState();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing ProfileViewModel: {ex.Message}");
                throw;
            }
        }

        // The currently logged-in user, or null if not authenticated
        public User CurrentUser => _userService.GetCurrentUser();

        // Flags for authentication state
        public bool IsAuthenticated => CurrentUser != null;
        public bool IsNotAuthenticated => !IsAuthenticated;

        // Navigate to the login page
        private async void OnNavigateToLogin()
        {
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }

        // Navigate to the registration page
        private async void OnNavigateToRegister()
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }

        // Log out the current user and update UI
        private async void OnLogout()
        {
            await _userService.LogoutAsync();
            UpdateUserState();
        }

        // Notify UI that user-related properties have changed
        public void UpdateUserState()
        {
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(IsNotAuthenticated));
        }
    }
}

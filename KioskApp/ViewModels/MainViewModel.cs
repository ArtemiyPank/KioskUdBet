using System.Diagnostics;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        public MainViewModel()
        {
            _userService = DependencyService.Get<IUserService>();
        }

        // Currently logged-in user
        public User CurrentUser => _userService.GetCurrentUser();

        // True if a user is authenticated
        public bool IsAuthenticated => CurrentUser != null;

        // True if no user is authenticated
        public bool IsNotAuthenticated => !IsAuthenticated;

        // True if the authenticated user has the Admin role
        public bool IsAdmin => CurrentUser?.Role == "Admin";

        // True if the authenticated user is not an admin
        public bool IsUser => !IsAdmin;

        // Notify the UI that user-related properties have changed
        public void UpdateUserState()
        {
            Debug.WriteLine("Updating user state in MainViewModel");
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(IsNotAuthenticated));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(IsUser));
        }
    }
}

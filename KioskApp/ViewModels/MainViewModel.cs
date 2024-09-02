using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;
using System;
using System.Diagnostics;

namespace KioskApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        public User CurrentUser => _userService.GetCurrentUser();
        public bool IsAuthenticated => CurrentUser != null;
        public bool IsNotAuthenticated => !IsAuthenticated;
        public bool IsAdmin { 
            get {
                Debug.WriteLine($"CurrentUser?.Role == \"Admin\" - {CurrentUser?.Role == "Admin"}");
                return CurrentUser?.Role == "Admin"; 
            } 
        }
        public bool IsUser => !IsAdmin;

        public MainViewModel()
        {
            _userService = DependencyService.Get<IUserService>();
        }

        public void UpdateUserState()
        {
            Debug.WriteLine("!!!!!!!!!!!!UpdateUserState!!!!!!!!!!!!");
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(IsNotAuthenticated));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(IsUser));
        }
    }
}

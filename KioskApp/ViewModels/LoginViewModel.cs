using System.Windows.Input;
using KioskApp.Services;
using KioskApp.Views;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        // Command to navigate to the registration page
        public ICommand NavigateToRegisterCommand { get; }

        // Command to perform login
        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _userService = DependencyService.Get<IUserService>();
            NavigateToRegisterCommand = new Command(OnNavigateToRegister);
            LoginCommand = new Command(async () => await LoginAsync());
        }

        private string email;
        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string password;
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }

        // Attempt to log in with the provided credentials
        public async Task<bool> LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Email and password are required.";
                return false;
            }

            var response = await _userService.AuthenticateAsync(Email, Password);
            if (response.IsSuccess)
            {
                await Shell.Current.GoToAsync("..");  // Navigate back on successful login
                return true;
            }

            ErrorMessage = response.Message;
            return false;
        }

        // Navigate to the RegisterPage
        private async void OnNavigateToRegister()
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}

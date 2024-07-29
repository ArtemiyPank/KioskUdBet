using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IUserApiService _apiService;
        private readonly IUserService _userService;

        public RegisterViewModel()
        {
            _apiService = DependencyService.Get<IUserApiService>();
            _userService = DependencyService.Get<IUserService>();
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged();
            }
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged();
            }
        }

        private string _building;
        public string Building
        {
            get => _building;
            set
            {
                _building = value;
                OnPropertyChanged();
            }
        }

        private string _roomNumber;
        public string RoomNumber
        {
            get => _roomNumber;
            set
            {
                _roomNumber = value;
                OnPropertyChanged();
            }
        }

        private string _language;
        public string Language
        {
            get => _language;
            set
            {
                _language = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged();
            }
        }

        public async Task<bool> RegisterUser()
        {
            var user = new User
            {
                Email = Email,
                Password = Password,
                FirstName = FirstName,
                LastName = LastName,
                Building = Building,
                RoomNumber = RoomNumber,
                Language = Language,
            };

            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) || string.IsNullOrEmpty(user.Building) || string.IsNullOrEmpty(user.RoomNumber) || string.IsNullOrEmpty(user.Language))
            {
                ErrorMessage = "All fields are required.";
                return false;
            }


            var registeredResponse = await _userService.Register(user);
            if (registeredResponse != null && registeredResponse.IsSuccess)
            {
                Debug.WriteLine($"Registration User in RegistrationViewModel: {registeredResponse.Data.Email}");
                await Shell.Current.GoToAsync("..");
                MessagingCenter.Send(this, "UpdateUserState");
                return true;
            }
            else
            {
                ErrorMessage = registeredResponse?.Message ?? "Registration failed.";
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

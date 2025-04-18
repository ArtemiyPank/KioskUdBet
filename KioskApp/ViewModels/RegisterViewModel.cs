using System.Windows.Input;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        public RegisterViewModel()
        {
            _userService = DependencyService.Get<IUserService>();
            SetPlaceOfBirthCommand = new Command(async () => await SetPlaceOfBirthByGeolocationAsync());
            RegisterCommand = new Command(async () => await RegisterUserAsync());
        }

        // Команда для автозаполнения места рождения
        public ICommand SetPlaceOfBirthCommand { get; }
        // Команда для регистрации
        public ICommand RegisterCommand { get; }

        // Поля и свойства для привязки
        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _building;
        public string Building
        {
            get => _building;
            set => SetProperty(ref _building, value);
        }

        private string _roomNumber;
        public string RoomNumber
        {
            get => _roomNumber;
            set => SetProperty(ref _roomNumber, value);
        }

        private string _language;
        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        private string _placeOfBirth;
        public string PlaceOfBirth
        {
            get => _placeOfBirth;
            set => SetProperty(ref _placeOfBirth, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Регистрация пользователя
        public async Task<bool> RegisterUserAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Building) ||
                string.IsNullOrWhiteSpace(RoomNumber) ||
                string.IsNullOrWhiteSpace(Language))
            {
                ErrorMessage = "All fields are required.";
                return false;
            }

            var user = new User
            {
                Email = Email,
                Password = Password,
                FirstName = FirstName,
                LastName = LastName,
                Building = Building,
                RoomNumber = RoomNumber,
                Language = Language,
                PlaceOfBirth = PlaceOfBirth
            };

            var response = await _userService.RegisterAsync(user);
            if (response.IsSuccess)
            {
                await Shell.Current.GoToAsync("..");
                return true;
            }

            ErrorMessage = response.Message;
            return false;
        }

        // Заполнить PlaceOfBirth через геолокацию
        public async Task SetPlaceOfBirthByGeolocationAsync()
        {
            try
            {
                var loc = await Geolocation.GetLastKnownLocationAsync()
                          ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
                if (loc != null)
                {
                    var marks = await Geocoding.GetPlacemarksAsync(loc);
                    var pm = marks?.FirstOrDefault();
                    if (pm != null)
                    {
                        PlaceOfBirth = $"{pm.Locality}, {pm.AdminArea}, {pm.CountryName}";
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Unable to get location: {ex.Message}",
                    "OK");
            }
        }
    }
}

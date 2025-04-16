using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KioskApp.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IUserApiService _apiService;
        private readonly IUserService _userService;

        public ICommand SetPlaceOfBirthCommand { get; }

        public RegisterViewModel()
        {
            _apiService = DependencyService.Get<IUserApiService>();
            _userService = DependencyService.Get<IUserService>();
            SetPlaceOfBirthCommand = new Command(async () => await SetPlaceOfBirthByGeolocationAsync());

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

        //private string _placeOfBirth;
        //public string PlaceOfBirth
        //{
        //    get => _placeOfBirth;
        //    set
        //    {
        //        _placeOfBirth = value;
        //        OnPropertyChanged();
        //    }
        //}

        private string _placeOfBirth;
        public string PlaceOfBirth
        {
            get => _placeOfBirth;
            set
            {
                SetProperty(ref _placeOfBirth, value);
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
                PlaceOfBirth = PlaceOfBirth
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


        public async Task SetPlaceOfBirthByGeolocationAsync()
        {
            try
            {
                // Получаем последнее известное местоположение
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    // Если последнее известное местоположение недоступно, запрашиваем новое
                    location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
                }
                if (location != null)
                {
                    // Выполняем обратное геокодирование для получения адреса
                    var placemarks = await Geocoding.GetPlacemarksAsync(location);
                    var placemark = placemarks?.FirstOrDefault();
                    if (placemark != null)
                    {
                        // Формируем читаемое представление адреса
                        string placeOfBirth = $"{placemark.Locality}, {placemark.AdminArea}, {placemark.CountryName}";
                        // Например, обновляем соответствующее свойство в модели
                        // Предполагаем, что у вас есть привязка к CurrentUser в ProfileViewModel
                        PlaceOfBirth = placeOfBirth;
                        OnPropertyChanged(nameof(PlaceOfBirth));
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок (например, отсутствие разрешений)
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Невозможно получить данные геолокации: {ex.Message}", "OK");
            }
        }
    }
}

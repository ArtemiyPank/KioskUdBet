using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IUserService _userService;

        public RegisterViewModel()
        {
            _apiService = DependencyService.Get<IApiService>();
            _userService = DependencyService.Get<IUserService>();
        }

        //private string _email;
        //public string Email
        //{
        //    get => _email;
        //    set
        //    {
        //        _email = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private string _password;
        //public string Password
        //{
        //    get => _password;
        //    set
        //    {
        //        _password = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private string _firstName;
        //public string FirstName
        //{
        //    get => _firstName;
        //    set
        //    {
        //        _firstName = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private string _lastName;
        //public string LastName
        //{
        //    get => _lastName;
        //    set
        //    {
        //        _lastName = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private string _building;
        //public string Building
        //{
        //    get => _building;
        //    set
        //    {
        //        _building = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private string _roomNumber;
        //public string RoomNumber
        //{
        //    get => _roomNumber;
        //    set
        //    {
        //        _roomNumber = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private string _language;
        //public string Language
        //{
        //    get => _language;
        //    set
        //    {
        //        _language = value;
        //        OnPropertyChanged();
        //    }
        //}

        public string Email { get; set; }
        public string Password { get; set; }
        public string Language { get; set; } // "English" / "Russian" / "Hebrew"
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Building { get; set; } // "Paz" / "Degel" / "Lavan / "Thelet"
        public string RoomNumber { get; set; }


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
            Debug.WriteLine($"Email: {Email}");

            Debug.WriteLine($"user: {user.ToString()}");

            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) || string.IsNullOrEmpty(user.Building) || string.IsNullOrEmpty(user.RoomNumber) || string.IsNullOrEmpty(user.Language))
            {
                Debug.WriteLine($"Not all fields are filled in");
                return false;
            }


            var registeredUser = await _userService.Register(user);
            Debug.WriteLine($"registeredUser: {registeredUser}");
            if (registeredUser != null)
            {

                Debug.WriteLine($"Registered User in RegisterViewModel: {registeredUser.Email}");
                await Shell.Current.GoToAsync(".."); // Переход на предыдущую страницу, ProfilePage должна быть предшествующей страницей
                MessagingCenter.Send(this, "UpdateUserState");

                return true;
            }
            else
            {
                // Registration error
                // Дописать логику
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

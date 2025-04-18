using System.Diagnostics;
using KioskApp.Services;
using KioskApp.Helpers;

namespace KioskApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Allow self-signed certificates for development
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            // Configure HttpClient for the backend API (Android emulator address)
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://10.0.2.2:7074")
            };

            // Register application-wide state
            var appState = new AppState();
            DependencyService.RegisterSingleton(appState);

            // Register API services
            var userApiService = new UserApiService(httpClient);
            DependencyService.RegisterSingleton<IUserApiService>(userApiService);
            DependencyService.RegisterSingleton<IProductApiService>(new ProductApiService(httpClient, userApiService));

            // Register domain logic services
            DependencyService.Register<IUserService, UserService>();

            MainPage = new AppShell();

            // Attempt to restore saved authentication
            ValidateSavedToken();
        }

        private async void ValidateSavedToken()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                    return;

                var userService = DependencyService.Get<IUserService>();
                var user = await userService.AuthenticateWithTokenAsync();
                if (user != null)
                {
                    userService.SetCurrentUser(user);
                    MessagingCenter.Send(this, "UserStateChanged");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error validating saved token: {ex.Message}");
            }
        }
    }
}

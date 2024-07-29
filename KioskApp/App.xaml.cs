using KioskApp.Services;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Net.Http;

namespace KioskApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Debug.WriteLine($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!Я РОДИЛСЯ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };

            var httpClient = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri("https://10.0.2.2:7074") // Для Android эмулятора
            };

            var userApiService = new UserApiService(httpClient);
            DependencyService.RegisterSingleton<IUserApiService>(userApiService);

            var productApiService = new ProductApiService(httpClient);
            DependencyService.RegisterSingleton<IProductApiService>(productApiService);

            DependencyService.Register<IUserService, UserService>();

            MainPage = new AppShell();

            CheckSavedToken();
        }

        private async void CheckSavedToken()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");

                Debug.WriteLine($"token: {token}");
                if (!string.IsNullOrEmpty(token))
                {
                    var userService = DependencyService.Get<IUserService>();
                    if (userService == null)
                    {
                        Debug.WriteLine("userService is null");
                        return;
                    }

                    var user = await userService.AuthenticateWithToken();
                    if (user != null)
                    {
                        userService.SetCurrentUser(user);
                        MessagingCenter.Send(this, "UpdateUserState");
                        Debug.WriteLine($"Token validated. Current User: {user.Email}");
                    }
                    else
                    {
                        Debug.WriteLine("Token validation failed.");
                    }
                }
                else
                {
                    Debug.WriteLine("No token found in SecureStorage.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving token: {ex.Message}");
            }
        }
    }
}

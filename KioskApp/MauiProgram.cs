using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using KioskApp.Services;
using KioskApp.Helpers;
using KioskApp.ViewModels;
using KioskApp.Views;

namespace KioskApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt5QHFqVkNrXVNbdV5dVGpAd0N3RGlcdlR1fUUmHVdTRHRbQlVgSX5UdUZmXHZceHY=;Mgo+DSMBPh8sVXJyS0d+X1RPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9nSXZQdkVkWnxbdX1RRWk=;Mgo+DSMBMAY9C3t2U1hhQlJBfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTX5bdUVjXnxddXFcQ2lZ;MzQ1MjYzNEAzMjM2MmUzMDJlMzBGRExORGxRQ1dST3A3WTdDbk1UdVY4OS82NlJkNStKMzNQSjFEWlFBeUMwPQ==");


            // Accept any SSL certificate (for development only)
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            // Configure HttpClient for the backend API
            var httpClient = new HttpClient(httpHandler)
            {
                BaseAddress = new Uri("https://10.0.2.2:7074")
            };
            builder.Services.AddSingleton(httpClient);

            // Global application state
            builder.Services.AddSingleton<AppState>();

            // API service registrations
            builder.Services.AddSingleton<IUserApiService, UserApiService>();
            builder.Services.AddSingleton<IProductApiService, ProductApiService>();
            builder.Services.AddSingleton<IOrderApiService, OrderApiService>();

            // Domain service registrations
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<ICacheService, CacheService>();
            builder.Services.AddSingleton<ISseService, SseService>();
            builder.Services.AddSingleton<IUpdateService, UpdateService>();

            // ViewModel registrations
            builder.Services.AddSingleton<ProfileViewModel>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<RegisterViewModel>();
            builder.Services.AddSingleton<ProductsViewModel>();
            builder.Services.AddSingleton<AddProductViewModel>();
            builder.Services.AddSingleton<EditProductViewModel>();
            builder.Services.AddSingleton<OrdersViewModel>();
            builder.Services.AddSingleton<CartViewModel>();

            // View registrations
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<ProductsPage>();
            builder.Services.AddTransient<AddProductPage>();
            builder.Services.AddTransient<EditProductPage>();
            builder.Services.AddTransient<OrdersPage>();
            builder.Services.AddTransient<CartPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

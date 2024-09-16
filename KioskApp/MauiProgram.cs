using KioskApp;
using KioskApp.Helpers;
using KioskApp.Services;
using KioskApp.ViewModels;
using KioskApp.Views;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using System.Net.Http;

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

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt5QHFqVkNrXVNbdV5dVGpAd0N3RGlcdlR1fUUmHVdTRHRbQlVgSX5UdUZmXHZceHY=;Mgo+DSMBPh8sVXJyS0d+X1RPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9nSXZQdkVkWnxbdX1RRWk=;Mgo+DSMBMAY9C3t2U1hhQlJBfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTX5bdUVjXnxddXFcQ2lZ;MzQ1MjYzNEAzMjM2MmUzMDJlMzBGRExORGxRQ1dST3A3WTdDbk1UdVY4OS82NlJkNStKMzNQSjFEWlFBeUMwPQ==");


        // HttpClient setup for Android emulator
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
        var httpClient = new HttpClient(httpClientHandler)
        {
            BaseAddress = new Uri("https://10.0.2.2:7074") // for emulator
            //BaseAddress = new Uri("http://10.114.64.23:4074")

        };
        // Register HttpClient and global state
        builder.Services.AddSingleton(httpClient);
        builder.Services.AddSingleton<AppState>();
        builder.Services.AddTransient<AppShell>();

        // Register services
        builder.Services.AddSingleton<IUserApiService, UserApiService>();
        builder.Services.AddSingleton<IProductApiService, ProductApiService>();
        builder.Services.AddSingleton<IOrderApiService, OrderApiService>();
        builder.Services.AddSingleton<IUserService, UserService>();
        builder.Services.AddSingleton<ICacheService, CacheService>(); 
        builder.Services.AddSingleton<ISseService, SseService>();
        builder.Services.AddSingleton<IUpdateService, UpdateService>();



        // Register view models
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ProductsViewModel>();
        builder.Services.AddTransient<AddProductViewModel>();
        builder.Services.AddTransient<EditProductViewModel>();
        builder.Services.AddTransient<OrdersViewModel>();
        builder.Services.AddSingleton<CartViewModel>();

        // Register views
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
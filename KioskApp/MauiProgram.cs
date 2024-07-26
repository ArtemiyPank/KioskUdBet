using KioskApp.Services;
using KioskApp.ViewModels;
using KioskApp.Views;
using Microsoft.Extensions.Logging;

namespace KioskApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register other services
            builder.Services.AddSingleton<IUserService, UserService>();

            // Register view models
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<ProductsViewModel>();
            builder.Services.AddTransient<CartViewModel>();

            // Register views
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<ProductsPage>();
            builder.Services.AddTransient<CartPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

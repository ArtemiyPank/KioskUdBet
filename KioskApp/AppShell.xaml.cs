using KioskApp.ViewModels;
using KioskApp.Views;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace KioskApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            Routing.RegisterRoute(nameof(ProductsPage), typeof(ProductsPage));
            Routing.RegisterRoute(nameof(AddProductPage), typeof(AddProductPage));
            Routing.RegisterRoute(nameof(EditProductPage), typeof(EditProductPage));
            Routing.RegisterRoute(nameof(CartPage), typeof(CartPage));

            // Subscribe to messages
            MessagingCenter.Subscribe<LoginViewModel>(this, "UpdateUserState", (sender) =>
            {
                UpdateProfilePage();
            });

            MessagingCenter.Subscribe<RegisterViewModel>(this, "UpdateUserState", (sender) =>
            {
                UpdateProfilePage();
            });

            MessagingCenter.Subscribe<ProfileViewModel>(this, "UpdateUserState", (sender) =>
            {
                UpdateProfilePage();
            });

        }

        private void UpdateProfilePage()
        {
            var profileViewModel = new ProfileViewModel();
            profileViewModel.UpdateUserState();
            Debug.WriteLine($"Updated Profile Page with User: {profileViewModel.CurrentUser?.Email}");
            if (Shell.Current.CurrentPage is ProfilePage profilePage)
            {
                profilePage.BindingContext = profileViewModel;
            }
        }
    }
}

using KioskApp.Services;
using KioskApp.ViewModels;
using KioskApp.Views;
using System.Diagnostics;

namespace KioskApp
{
    public partial class AppShell : Shell
    {
        private readonly IUserService _userService;

        public AppShell()
        {
            InitializeComponent();

            _userService = DependencyService.Get<IUserService>();

            // Register navigation routes
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            Routing.RegisterRoute(nameof(ProductsPage), typeof(ProductsPage));
            Routing.RegisterRoute(nameof(AddProductPage), typeof(AddProductPage));
            Routing.RegisterRoute(nameof(EditProductPage), typeof(EditProductPage));
            Routing.RegisterRoute(nameof(OrdersPage), typeof(OrdersPage));
            Routing.RegisterRoute(nameof(CartPage), typeof(CartPage));

            // Listen for user login/logout events
            MessagingCenter.Subscribe<UserService>(this, "UserStateChanged", sender =>
            {
                UpdateProfilePage();
                UpdateTabVisibility();
            });
        }

        // Update the profile page view model when user state changes
        private void UpdateProfilePage()
        {
            var profileVm = new ProfileViewModel();
            profileVm.UpdateUserState();
            Debug.WriteLine($"Profile updated for: {profileVm.CurrentUser?.Email}");

            if (CurrentPage is ProfilePage page)
            {
                page.BindingContext = profileVm;
            }

            // Notify other pages of user state change
            MessagingCenter.Send(this, "UserStateChanged");
        }

        // Show admin tabs or cart tab based on the user's role
        private void UpdateTabVisibility()
        {
            var user = _userService.GetCurrentUser();

            if (user?.Role == "Admin")
            {
                CartTab.IsVisible = false;
                OrdersTab.IsVisible = true;
            }
            else
            {
                CartTab.IsVisible = true;
                OrdersTab.IsVisible = false;
            }
        }
    }
}

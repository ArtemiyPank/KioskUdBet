using KioskApp.Services;
using KioskApp.ViewModels;
using KioskApp.Views;
using Microsoft.Maui.Controls;
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

            // Register routes
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            Routing.RegisterRoute(nameof(ProductsPage), typeof(ProductsPage));
            Routing.RegisterRoute(nameof(AddProductPage), typeof(AddProductPage));
            Routing.RegisterRoute(nameof(EditProductPage), typeof(EditProductPage));
            Routing.RegisterRoute(nameof(OrdersPage), typeof(OrdersPage));
            Routing.RegisterRoute(nameof(CartPage), typeof(CartPage));


            MessagingCenter.Subscribe<UserService>(this, "UserStateChanged", (sender) =>
            {
                Debug.WriteLine("UserService UserStateChanged");
                UpdateProfilePage();
                UpdateTabsBasedOnUserRole();
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
            MessagingCenter.Send(this, "UserStateChanged"); // To update the product page
        }




        private void UpdateTabsBasedOnUserRole()
        {
            var currentUser = _userService.GetCurrentUser();

            if (currentUser?.Role == "Admin")
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



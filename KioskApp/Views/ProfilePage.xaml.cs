using KioskApp.ViewModels;

namespace KioskApp.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileViewModel _viewModel;

        // Constructor: receives the ViewModel via DI and sets up the BindingContext
        public ProfilePage()
        {
            InitializeComponent();
            BindingContext = new ProfileViewModel();

            // Subscribe to user state changes and update the ViewModel accordingly
            MessagingCenter.Subscribe<AppShell>(this, "UserStateChanged", (sender) =>
            {

                BindingContext = new ProfileViewModel();
            });
        }
    }
}

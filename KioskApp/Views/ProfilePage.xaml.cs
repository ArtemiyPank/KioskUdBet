using KioskApp.ViewModels;
using Microsoft.Maui.Controls;

namespace KioskApp.Views
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
            BindingContext = new ProfileViewModel();

            MessagingCenter.Subscribe<AppShell>(this, "UserStateChanged", (sender) =>
            {

                BindingContext = new ProfileViewModel();
            });
        }
    }
}

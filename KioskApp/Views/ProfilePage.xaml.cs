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

            MessagingCenter.Subscribe<ProfileViewModel>(this, "UpdateUserState", (sender) =>
            {
                BindingContext = new ProfileViewModel();
            });
        }
    }
}

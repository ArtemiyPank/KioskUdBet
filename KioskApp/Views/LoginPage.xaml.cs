using KioskApp.ViewModels;
using Microsoft.Maui.Controls;

namespace KioskApp.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel();
        }

        private void OnShowPasswordCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            passwordEntry.IsPassword = !e.Value;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var viewModel = BindingContext as LoginViewModel;
            if (viewModel != null)
            {
                var result = await viewModel.Login();
                if (result)
                {
                    // Navigate to profile page or other appropriate action
                    await Navigation.PushAsync(new ProfilePage());
                }
                else
                {
                    await DisplayAlert("Error", viewModel.ErrorMessage, "OK");
                }
            }
        }
    }
}

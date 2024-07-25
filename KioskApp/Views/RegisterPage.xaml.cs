using KioskApp.ViewModels;
using Microsoft.Maui.Controls;

namespace KioskApp.Views
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
            BindingContext = new RegisterViewModel();
        }

        private void OnShowPasswordCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            passwordEntry.IsPassword = !e.Value;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var viewModel = BindingContext as RegisterViewModel;
            if (viewModel != null)
            {
                var result = await viewModel.RegisterUser();
                if (result)
                {
                    await DisplayAlert("Success", "Registration successful", "OK");
                }
                else
                {
                    await DisplayAlert("Error", viewModel.ErrorMessage, "OK");
                }
            }
        }
    }
}

using KioskApp.ViewModels;

namespace KioskApp.Views
{
    // Code‑behind for the RegisterPage XAML view
    public partial class RegisterPage : ContentPage
    {
        // Initialize components and set the ViewModel
        public RegisterPage()
        {
            InitializeComponent();
            BindingContext = new RegisterViewModel();
        }

        // Toggle the visibility of the password field
        private void OnShowPasswordCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            passwordEntry.IsPassword = !e.Value;
        }

        // Handle the Register button click
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            if (BindingContext is RegisterViewModel viewModel)
            {
                bool isRegistered = await viewModel.RegisterUserAsync();
                if (isRegistered)
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

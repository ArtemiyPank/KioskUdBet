using KioskApp.ViewModels;

namespace KioskApp.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel();
        }

        // Toggle the password field visibility when the checkbox is changed
        private void OnShowPasswordCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            passwordEntry.IsPassword = !e.Value;
        }

        // Handle the Login button click
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (BindingContext is LoginViewModel viewModel)
            {
                bool success = await viewModel.LoginAsync();
                if (success)
                {
                    // Navigate to the profile page on successful login
                    await Navigation.PushAsync(new ProfilePage());
                }
                else
                {
                    // Show an error alert if login failed
                    await DisplayAlert("Error", viewModel.ErrorMessage, "OK");
                }
            }
        }
    }
}

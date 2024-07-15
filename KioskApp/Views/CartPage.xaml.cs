using KioskApp.ViewModels;
using KioskApp.Services;

namespace KioskApp.Views
{
    public partial class CartPage : ContentPage
    {
        public CartPage()
        {
            InitializeComponent();
            BindingContext = new CartViewModel(DependencyService.Get<IApiService>());
        }
    }
}

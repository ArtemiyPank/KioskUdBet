using KioskApp.ViewModels;

namespace KioskApp.Views
{
    // Code-behind for the ProductsPage XAML view
    public partial class ProductsPage : ContentPage
    {
        // Constructor injects the ViewModel and sets it as the BindingContext
        public ProductsPage(ProductsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}

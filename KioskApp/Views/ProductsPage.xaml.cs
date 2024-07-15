using KioskApp.ViewModels;
using KioskApp.Services;

namespace KioskApp.Views
{
    public partial class ProductsPage : ContentPage
    {
        public ProductsPage(ProductsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public ProductsPage()
        {
            InitializeComponent();
            BindingContext = new ProductsViewModel(DependencyService.Get<IApiService>());
        }
    }
}

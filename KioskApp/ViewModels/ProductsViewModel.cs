using System.Collections.ObjectModel;
using System.Windows.Input;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;

        public ProductsViewModel(IApiService apiService)
        {
            _apiService = apiService;
            Products = new ObservableCollection<Product>();
            LoadProductsCommand = new Command(async () => await LoadProducts());
            AddToCartCommand = new Command<Product>(OnAddToCart);

            LoadProductsCommand.Execute(null);
        }

        public ObservableCollection<Product> Products { get; }
        public ICommand LoadProductsCommand { get; }
        public ICommand AddToCartCommand { get; }

        private async Task LoadProducts()
        {
            var products = await _apiService.GetProducts();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }

        private void OnAddToCart(Product product)
        {
            // Add product to the cart
        }
    }
}

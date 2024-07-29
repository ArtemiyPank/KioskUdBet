using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using KioskApp.Models;
using KioskApp.Services;
using KioskApp.Views;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly CacheService _cacheService;

        private Stream _imageStream;
        private string _imageName;

        public ObservableCollection<Product> Products { get; private set; }
        public Product NewProduct { get; set; }
        public ICommand LoadProductsCommand { get; private set; }
        public ICommand NavigateToAddProductCommand { get; private set; }
        public ICommand AddToCartCommand { get; private set; }


        public ProductsViewModel()
        {
            _apiService = DependencyService.Get<IApiService>();
            _cacheService = new CacheService();
            Initialize();

            MessagingCenter.Subscribe<AddProductViewModel>(this, "ProductAdded", async (sender) =>
            {
                await LoadProducts();
            });
        }


        private void Initialize()
        {
            Debug.WriteLine("ТВОЮ МААААААААААТЬ Initialize");

            _cacheService.ClearCache();
            Products = new ObservableCollection<Product>();
            NewProduct = new Product();
            LoadProductsCommand = new Command(async () => await LoadProducts());
            NavigateToAddProductCommand = new Command(async () => await NavigateToAddProduct());
            AddToCartCommand = new Command<Product>(OnAddToCart);

            LoadProductsCommand.Execute(null);
            Debug.WriteLine("ТВОЮ МААААААААААТЬ Initialize END");

        }


        private async Task LoadProducts()
        {
            try
            {
                var products = await _apiService.GetProducts();
                Products.Clear();

                var cachedProductIds = _cacheService.GetCachedProductIds();

                foreach (var product in products)
                {
                    cachedProductIds.Remove(product.Id); // Удалить товар из списка кэшированных ID, если он существует в базе данных

                    var cachedProduct = await _cacheService.GetProductAsync(product.Id);
                    if (cachedProduct != null)
                    {
                        if (cachedProduct.LastUpdated != product.LastUpdated) // Проверка, изменился ли товар
                        {
                            Debug.WriteLine($"Updating cache for product {product.Id}");
                            var imageStream = await _apiService.DownloadProductImage(product.ImageUrl);
                            await _cacheService.SaveProductAsync(product, imageStream);
                        }
                        product.ImageUrl = _cacheService.GetProductImagePath(product.Id);
                    }
                    else
                    {
                        Debug.WriteLine($"Caching new product {product.Id}");
                        var imageStream = await _apiService.DownloadProductImage(product.ImageUrl);
                        await _cacheService.SaveProductAsync(product, imageStream);
                        product.ImageUrl = _cacheService.GetProductImagePath(product.Id);
                    }
                    Products.Add(product);
                }

                // Удаление несуществующих товаров из кэша
                foreach (var productId in cachedProductIds)
                {
                    Debug.WriteLine($"Deleting cache for non-existing product {productId}");
                    _cacheService.DeleteProduct(productId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading products: {ex.Message}");
            }
        }

        private async Task NavigateToAddProduct()
        {
            await Shell.Current.GoToAsync(nameof(AddProductPage));
        }



        private void OnAddToCart(Product product)
        {
            // Add product to the cart
        }
    }
}

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using KioskApp.Views;

namespace KioskApp.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly IProductApiService _productApiService;
        private readonly CacheService _cacheService;


        public ObservableCollection<Product> Products { get; private set; }
        public ICommand LoadProductsCommand { get; private set; }
        public ICommand NavigateToAddProductCommand { get; private set; }

        public ICommand DeleteProductCommand { get; private set; }
        public ICommand NavigateToEditProductCommand { get; private set; }
        public ICommand HideProductCommand { get; private set; }

        public ProductsViewModel()
        {
            _productApiService = DependencyService.Get<IProductApiService>();
            _cacheService = new CacheService();


            Products = new ObservableCollection<Product>();

            LoadProductsCommand = new Command(async () => await LoadProducts());
            
            NavigateToAddProductCommand = new Command(async () => await NavigateToAddProduct());
            DeleteProductCommand = new Command<Product>(async (product) => await DeleteProduct(product));
            NavigateToEditProductCommand = new Command<Product>(async (product) => await NavigateToEditProduct(product));
            HideProductCommand = new Command<Product>(async (product) => await HideProduct(product));

            LoadProductsCommand.Execute(null);


            MessagingCenter.Subscribe<AddProductViewModel>(this, "ProductAdded", async (sender) =>
            {
                await LoadProducts();
            });

            MessagingCenter.Subscribe<EditProductViewModel>(this, "ProductUpdated", async (sender) =>
            {
                await LoadProducts();
            });
        }



        

        private async Task LoadProducts()
        {
            try
            {
                //_cacheService.LogProductCache();

                var products = await _productApiService.GetProducts();
                Products.Clear();

                var cachedProductIds = _cacheService.GetCachedProductIds();

                foreach (var product in products)
                {
                    cachedProductIds.Remove(product.Id);

                    var cachedProduct = await _cacheService.GetProductAsync(product.Id);
                    if (cachedProduct != null)
                    {
                        if (cachedProduct.LastUpdated != product.LastUpdated)
                        {
                            var imageStream = await _productApiService.DownloadProductImage(product.ImageUrl);
                            await _cacheService.SaveProductAsync(product, imageStream);
                        }
                        product.ImageUrl = _cacheService.GetProductImagePath(product.Id);
                    }
                    else
                    {
                        var imageStream = await _productApiService.DownloadProductImage(product.ImageUrl);
                        await _cacheService.SaveProductAsync(product, imageStream);
                        product.ImageUrl = _cacheService.GetProductImagePath(product.Id);
                    }
                    Products.Add(product);
                }

                foreach (var productId in cachedProductIds)
                {
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

        private async Task NavigateToEditProduct(Product product)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Product", product }
            };
            await Shell.Current.GoToAsync(nameof(EditProductPage), navigationParameter);
        }


        private async Task HideProduct(Product product)
        {
            var result = await _productApiService.HideProduct(product.Id);
            if (result)
            {
                product.IsHidden = true;
                await LoadProducts();
                await LoadProducts();
            }
        }

        private async Task DeleteProduct(Product product)
        {
            Debug.WriteLine($"product - {product.ToString()}");

            var result = await _productApiService.DeleteProduct(product.Id);
            if (result)
            {
                Products.Remove(product);
                await LoadProducts();
            }
        }


    }
}

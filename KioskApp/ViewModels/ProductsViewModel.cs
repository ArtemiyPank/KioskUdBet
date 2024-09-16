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
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly CartViewModel _cartViewModel;
        private readonly ISseService _sseService;

        private CancellationTokenSource _cts;

        public ObservableCollection<Product> Products { get; private set; }
        public ICommand LoadProductsCommand { get; private set; }
        public ICommand NavigateToAddProductCommand { get; private set; }
        public ICommand DeleteProductCommand { get; private set; }
        public ICommand NavigateToEditProductCommand { get; private set; }
        public ICommand ToggleVisibilityCommand { get; private set; }
        public ICommand AddToCartCommand { get; }

        public bool IsAdmin
        {
            get
            {
                if (_userService.GetCurrentUser() != null)
                {
                    return _userService.GetCurrentUser()?.Role == "Admin";
                }
                else { return false; }
            }
        }

        public ProductsViewModel(IProductApiService productApiService, IUserService userService, ICacheService cacheService, CartViewModel cartViewModel, ISseService sseService)
        {
            _productApiService = productApiService;
            _userService = userService;
            _cacheService = cacheService;
            _cartViewModel = cartViewModel;
            _sseService = sseService;

            Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

            Products = new ObservableCollection<Product>();

            AddToCartCommand = new Command<Product>((product) => OnAddToCart(product));
            LoadProductsCommand = new Command(async () => await LoadProducts());

            NavigateToAddProductCommand = new Command(async () => await NavigateToAddProduct());
            DeleteProductCommand = new Command<Product>(async (product) => await DeleteProduct(product));
            NavigateToEditProductCommand = new Command<Product>(async (product) => await NavigateToEditProduct(product));
            ToggleVisibilityCommand = new Command<Product>(async (product) => await ToggleVisibility(product));

            LoadProductsCommand.Execute(null);

            MessagingCenter.Subscribe<AddProductViewModel>(this, "ProductAdded", async (sender) =>
            {
                await LoadProducts();
            });

            MessagingCenter.Subscribe<EditProductViewModel>(this, "ProductUpdated", async (sender) =>
            {
                await LoadProducts();
            });

            // Loading products when user registers, logs in or logs out of account
            MessagingCenter.Subscribe<AppShell>(this, "UserStateChanged", async (sender) =>
            {
                OnPropertyChanged(nameof(IsAdmin));
                await LoadProducts();
            });
        }

        private async Task LoadProducts()
        {
            try
            {
                StopMonitoringProducts();

                Products.Clear(); // Clear the list before loading new data

                var products = await _productApiService.GetProducts();
                var cachedProductIds = _cacheService.GetCachedProductIds();

                foreach (var product in products)
                {
                    cachedProductIds.Remove(product.Id);

                    if (!IsAdmin && product.IsHidden)
                    {
                        continue;
                    }

                    var cachedProduct = await _cacheService.GetProductAsync(product.Id);
                    if (cachedProduct != null)
                    {
                        // Если изменился дата последнего обновления
                        if (cachedProduct.LastUpdated != product.LastUpdated)
                        {
                            var imageStream = await _productApiService.DownloadProductImage(product.ImageUrl);
                            await _cacheService.SaveProductAsync(product, imageStream);
                        }
                        product.ImageUrl = await _cacheService.GetProductImagePath(product.Id);
                    }
                    else
                    {
                        var imageStream = await _productApiService.DownloadProductImage(product.ImageUrl);
                        await _cacheService.SaveProductAsync(product, imageStream);
                        product.ImageUrl = await _cacheService.GetProductImagePath(product.Id);
                    }
                    Products.Add(product);
                }

                foreach (var productId in cachedProductIds)
                {
                    await _cacheService.DeleteProduct(productId);
                }

                Debug.WriteLine("Before StartMonitoringProductsStock");

                StartMonitoringProducts();

                Debug.WriteLine("After StartMonitoringProductsStock");


                _cacheService.PrintCacheDirectoryStructure(); // печать структуры папки кэша

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading products: {ex.Message}");
            }
        }


        public void StartMonitoringProducts()
        {
            Debug.WriteLine("In StartMonitoringProducts");
            _cts = new CancellationTokenSource();
            _sseService.StartMonitoringAllProductsStock(UpdateStock, _cts.Token);
        }


        public void StopMonitoringProducts()
        {
            Debug.WriteLine("In StopMonitoringProducts");

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }


        private void UpdateStock(int id, int quantity)
        {
            Debug.WriteLine($"In UpdateStock of product {id} with quantity {quantity}");
            foreach (var product in Products)
            {
                if (product.Id == id)
                {
                    Debug.WriteLine($"Product was found");

                    product.Stock = quantity;
                    OnPropertyChanged(nameof(Products));
                    return;
                }
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

        private async Task ToggleVisibility(Product product)
        {
            var result = await _productApiService.ToggleVisibility(product.Id);
            if (result)
            {
                product.IsHidden = !product.IsHidden;
                await LoadProducts();
            }
        }

        private async Task DeleteProduct(Product product)
        {
            var result = await _productApiService.DeleteProduct(product.Id);
            if (result)
            {
                Products.Remove(product);
                await LoadProducts();
            }
        }

        private void OnAddToCart(Product product)
        {
            Debug.WriteLine($"In OnAddToCart");

            // Добавляем продукт в корзину, используя CartViewModel
            _cartViewModel.AddToCartCommand.Execute(product);
            Debug.WriteLine($"Product added to cart: {product.Name}");
        }
    }
}

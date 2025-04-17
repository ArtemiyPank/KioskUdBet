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
using KioskApp.Helpers;

namespace KioskApp.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly IProductApiService _productApiService;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly CartViewModel _cartViewModel;
        private readonly ISseService _sseService;
        private readonly AppState _appState;

        private CancellationTokenSource? _cts;

        public ObservableCollection<Product> Products => _appState.Products;

        public ICommand LoadProductsCommand { get; private set; }
        public ICommand NavigateToAddProductCommand { get; private set; }
        public ICommand DeleteProductCommand { get; private set; }
        public ICommand NavigateToEditProductCommand { get; private set; }
        public ICommand ToggleVisibilityCommand { get; private set; }
        public ICommand AddToCartCommand { get; }
        public ICommand SpeakDescriptionCommand { get; }

        public bool IsAdmin
        {
            get
            {
                var currentUser = _userService.GetCurrentUser();
                Debug.WriteLine($"Checking if user is admin: {(currentUser?.Role == "Admin")}");
                return currentUser?.Role == "Admin";
            }
        }

        public ProductsViewModel(IProductApiService productApiService, IUserService userService, ICacheService cacheService, CartViewModel cartViewModel, ISseService sseService, AppState appState)
        {
            _productApiService = productApiService;
            _userService = userService;
            _cacheService = cacheService;
            _cartViewModel = cartViewModel;
            _sseService = sseService;
            _appState = appState;

            _appState.TestStr = "was initialized in ProductsViewModel";


            AddToCartCommand = new Command<Product>((product) => OnAddToCart(product));
            LoadProductsCommand = new Command(async () => await LoadProducts());

            NavigateToAddProductCommand = new Command(async () => await NavigateToAddProduct());
            DeleteProductCommand = new Command<Product>(async (product) => await DeleteProduct(product));
            NavigateToEditProductCommand = new Command<Product>(async (product) => await NavigateToEditProduct(product));
            ToggleVisibilityCommand = new Command<Product>(async (product) => await ToggleVisibility(product));
            SpeakDescriptionCommand = new Command<Product>(async product => await SpeakDescriptionAsync(product));

            Debug.WriteLine("ProductsViewModel initialized.");
            LoadProductsCommand.Execute(null);

            MessagingCenter.Subscribe<AddProductViewModel>(this, "ProductAdded", async (sender) =>
            {
                Debug.WriteLine("Product added, reloading products.");
                await LoadProducts();
            });

            MessagingCenter.Subscribe<EditProductViewModel>(this, "ProductUpdated", async (sender) =>
            {
                Debug.WriteLine("Product updated, reloading products.");
                await LoadProducts();
            });

            MessagingCenter.Subscribe<AppShell>(this, "UserStateChanged", async (sender) =>
            {
                Debug.WriteLine("User state changed, reloading products.");
                OnPropertyChanged(nameof(IsAdmin));
                await LoadProducts();
            });
        }

        public async Task LoadProducts()
        {
            try
            {
                Debug.WriteLine("Loading products...");
                StopMonitoringProducts();

                _appState.Products.Clear(); // Clear the list before loading new data

                var products = await _productApiService.GetProducts();
                var cachedProductIds = _cacheService.GetCachedProductIds();

                foreach (var product in products)
                {
                    cachedProductIds.Remove(product.Id);
                    Debug.WriteLine($"Processing product {product.Id}: {product.Name}");

                    if (!IsAdmin && product.IsHidden)
                    {
                        Debug.WriteLine($"Product {product.Id} is hidden and user is not admin. Skipping.");
                        continue;
                    }

                    var cachedProduct = await _cacheService.GetProductAsync(product.Id);
                    if (cachedProduct != null)
                    {
                        Debug.WriteLine($"Product {product.Id} found in cache.");
                        if (cachedProduct.LastUpdated != product.LastUpdated)
                        {
                            Debug.WriteLine($"Product {product.Id} has been updated, downloading new image.");
                            var imageStream = await _productApiService.DownloadProductImage(product.ImageUrl);
                            await _cacheService.SaveProductAsync(product, imageStream);
                        }
                        product.ImageUrl = await _cacheService.GetProductImagePath(product.Id);
                    }
                    else
                    {
                        Debug.WriteLine($"Product {product.Id} not found in cache, downloading image.");

                        var imageStream = await _productApiService.DownloadProductImage(product.ImageUrl);

                        await _cacheService.SaveProductAsync(product, imageStream);

                        product.ImageUrl = await _cacheService.GetProductImagePath(product.Id);
                    }
                    Debug.WriteLine(product.ToString());

                    _appState.Products.Add(product);
                }

                Debug.WriteLine("Finished loading products.");
                StartMonitoringProducts();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading products: {ex.Message}");
            }
        }




        public void StartMonitoringProducts()
        {
            _cts = new CancellationTokenSource();
            Debug.WriteLine("Started monitoring product stock via SSE.");
            _sseService.StartMonitoringAllProductsStock(UpdateStock, _cts.Token);
        }

        public void StopMonitoringProducts()
        {
            if (_cts != null)
            {
                Debug.WriteLine("Stopped monitoring product stock.");
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private void UpdateStock(int productId, int stock, int reservedStock)
        {
            Debug.WriteLine($"Updating stock for product {productId}. Stock: {stock}, Reserved: {reservedStock}");
            var product = Products.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                product.Stock = stock;
                product.ReservedStock = reservedStock;  // Обновляем зарезервированное количество
                OnPropertyChanged(nameof(Products));
            }
        }


        private async Task NavigateToAddProduct()
        {
            Debug.WriteLine("Navigating to AddProductPage.");
            await Shell.Current.GoToAsync(nameof(AddProductPage));
        }

        private async Task NavigateToEditProduct(Product product)
        {
            Debug.WriteLine($"Navigating to EditProductPage for product {product.Id}.");
            var navigationParameter = new Dictionary<string, object>
            {
                { "Product", product }
            };
            await Shell.Current.GoToAsync(nameof(EditProductPage), navigationParameter);
        }

        private async Task ToggleVisibility(Product product)
        {
            Debug.WriteLine($"Toggling visibility for product {product.Id}.");
            var result = await _productApiService.ToggleVisibility(product.Id);
            if (result)
            {
                product.IsHidden = !product.IsHidden;
                Debug.WriteLine($"Product {product.Id} visibility changed to {product.IsHidden}.");
                await LoadProducts();
            }
        }

        private async Task DeleteProduct(Product product)
        {
            Debug.WriteLine($"Deleting product {product.Id}.");
            var result = await _productApiService.DeleteProduct(product.Id);
            if (result)
            {
                Debug.WriteLine($"Product {product.Id} deleted.");
                _appState.Products.Remove(product);
                await LoadProducts();
            }
        }

        private void OnAddToCart(Product product)
        {
            Debug.WriteLine($"Adding product {product.Id} to cart.");
            _cartViewModel.AddToCartCommand.Execute(product);
        }

        public async Task ConfirmOrder(int productId, int quantity)
        {
            Debug.WriteLine($"Confirming order for product {productId} with quantity {quantity}.");
            await _productApiService.ConfirmOrder(productId, quantity);
        }

        private async Task SpeakDescriptionAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product?.Description))
                return;

            try
            {
                var settings = new SpeechOptions
                {
                    Volume = 1.0f,
                    Pitch = 1.0f
                };
                await TextToSpeech.Default.SpeakAsync(product.Description, settings);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TTS error: {ex.Message}");
            }
        }


        public void PrintProducts()
        {
            Debug.WriteLine("==================== Products in ProductsViewModel ====================");
            foreach (var product in Products)
            {
                Debug.WriteLine(product.ToString());
            }
            Debug.WriteLine("=======================================================================");
        }
    }
}

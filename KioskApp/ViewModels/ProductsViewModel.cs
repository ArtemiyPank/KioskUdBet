using KioskApp.Helpers;
using KioskApp.Models;
using KioskApp.Services;
using KioskApp.Views;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

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
        private CancellationTokenSource _cts;

        // Backing collection is in AppState so it's shared across viewmodels
        public ObservableCollection<Product> Products => _appState.Products;

        // Commands bound to the UI
        public ICommand LoadProductsCommand { get; }
        public ICommand ReloadProductsCommand { get; }
        public ICommand NavigateToAddProductCommand { get; }
        public ICommand NavigateToEditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand ToggleVisibilityCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand SpeakDescriptionCommand { get; }

        // Only admins may see hidden products
        public bool IsAdmin => _userService.GetCurrentUser()?.Role == "Admin";

        public ProductsViewModel(
            IProductApiService productApiService,
            IUserService userService,
            ICacheService cacheService,
            CartViewModel cartViewModel,
            ISseService sseService,
            AppState appState)
        {
            _productApiService = productApiService;
            _userService = userService;
            _cacheService = cacheService;
            _cartViewModel = cartViewModel;
            _sseService = sseService;
            _appState = appState;

            LoadProductsCommand = new Command(async () => await LoadProductsAsync());
            ReloadProductsCommand = new Command(async () => await LoadProductsAsync());
            NavigateToAddProductCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(AddProductPage)));
            NavigateToEditProductCommand = new Command<Product>(async p => await NavigateToEditProductAsync(p));
            DeleteProductCommand = new Command<Product>(async p => await DeleteProductAsync(p));
            ToggleVisibilityCommand = new Command<Product>(async p => await ToggleVisibilityAsync(p));
            AddToCartCommand = new Command<Product>(p => _cartViewModel.AddToCartCommand.Execute(p));
            SpeakDescriptionCommand = new Command<Product>(async p => await SpeakDescriptionAsync(p));

            // Automatically load products and start monitoring
            LoadProductsCommand.Execute(null);

            // Refresh list on relevant messages
            MessagingCenter.Subscribe<AddProductViewModel>(this, "ProductAdded", async _ => await LoadProductsAsync());
            MessagingCenter.Subscribe<EditProductViewModel>(this, "ProductUpdated", async _ => await LoadProductsAsync());
            MessagingCenter.Subscribe<AppShell>(this, "UserStateChanged", async _ =>
            {
                OnPropertyChanged(nameof(IsAdmin));
                await LoadProductsAsync();
            });
        }

        // Fetch products, apply caching, and start SSE monitoring
        private async Task LoadProductsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            StopMonitoringProducts();
            Products.Clear();

            try
            {
                var remoteList = await _productApiService.GetProductsAsync();
                var cachedIds = new HashSet<int>(_cacheService.GetCachedProductIds());

                foreach (var product in remoteList)
                {
                    if (!IsAdmin && product.IsHidden)
                        continue;

                    // Sync cache
                    if (cachedIds.Contains(product.Id))
                    {
                        var cached = await _cacheService.GetProductAsync(product.Id);
                        if (cached.LastUpdated != product.LastUpdated)
                        {
                            var stream = await _productApiService.DownloadProductImageAsync(product.ImageUrl);
                            await _cacheService.SaveProductAsync(product, stream);
                        }
                    }
                    else
                    {
                        var stream = await _productApiService.DownloadProductImageAsync(product.ImageUrl);
                        await _cacheService.SaveProductAsync(product, stream);
                    }

                    product.ImageUrl = await _cacheService.GetProductImagePath(product.Id);
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading products: {ex.Message}");
            }
            finally
            {
                StartMonitoringProducts();
                IsBusy = false;
            }
        }

        // Begin listening to server-sent events for real-time stock updates
        private void StartMonitoringProducts()
        {
            _cts = new CancellationTokenSource();
            _sseService.StartMonitoringAllProductsStockAsync(UpdateStock, _cts.Token);
        }

        // Stop listening to SSE updates
        private void StopMonitoringProducts()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        // Update a single product's stock values
        private void UpdateStock(int productId, int stock, int reservedStock)
        {
            var prod = Products.FirstOrDefault(p => p.Id == productId);
            if (prod != null)
            {
                prod.Stock = stock;
                prod.ReservedStock = reservedStock;
                OnPropertyChanged(nameof(Products));
            }
        }

        // Navigate to edit page, passing the selected product
        private async Task NavigateToEditProductAsync(Product product)
        {
            var navParams = new Dictionary<string, object> { ["Product"] = product };
            await Shell.Current.GoToAsync(nameof(EditProductPage), navParams);
        }

        // Toggle visibility via API and refresh list
        private async Task ToggleVisibilityAsync(Product product)
        {
            if (await _productApiService.ToggleVisibilityAsync(product.Id))
            {
                product.IsHidden = !product.IsHidden;
                await LoadProductsAsync();
            }
        }

        // Delete via API and refresh list
        private async Task DeleteProductAsync(Product product)
        {
            if (await _productApiService.DeleteProductAsync(product.Id))
            {
                Products.Remove(product);
            }
        }

        // Text-to-speech for product descriptions
        private async Task SpeakDescriptionAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product?.Description)) return;

            try
            {
                var settings = new SpeechOptions { Volume = 1.0f, Pitch = 1.0f };
                await TextToSpeech.Default.SpeakAsync(product.Description, settings);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TTS error: {ex.Message}");
            }
        }
    }
}

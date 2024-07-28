using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using KioskApp.Models;
using KioskApp.Services;
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
        public ICommand AddProductCommand { get; private set; }
        public ICommand ChooseImageCommand { get; private set; }
        public ICommand AddToCartCommand { get; private set; }


        public ProductsViewModel()
        {
            _apiService = DependencyService.Get<IApiService>();
            _cacheService = new CacheService();
            Initialize();
        }

        public ProductsViewModel(IApiService apiService)
        {
            _apiService = apiService;
            _cacheService = new CacheService();
            Initialize();
        }

        private void Initialize()
        {
            _cacheService.ClearCache();

            Products = new ObservableCollection<Product>();
            NewProduct = new Product();
            LoadProductsCommand = new Command(async () => await LoadProducts());
            Debug.WriteLine("ТВОЮ МААААААААААТЬ Initialize 1");
            AddProductCommand = new Command(async () => await AddProduct());
            Debug.WriteLine("ТВОЮ МААААААААААТЬ Initialize 2");
            ChooseImageCommand = new Command(async () => await ChooseImage());
            Debug.WriteLine("ТВОЮ МААААААААААТЬ Initialize 3");
            AddToCartCommand = new Command<Product>(OnAddToCart);

            Debug.WriteLine("ТВОЮ МААААААААААТЬ Initialize 4");
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


        private async Task AddProduct()
        {
            try
            {
                NewProduct.ImageUrl = "";
                await _apiService.AddProduct(NewProduct, _imageStream, _imageName);
                Products.Add(NewProduct);
                NewProduct = new Product(); // Очистка формы после добавления продукта
                _imageStream?.Dispose(); // Закрытие потока после использования
                _imageStream = null; // Сброс переменной после использования

                await LoadProducts(); // Обновление списка продуктов
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HttpRequestException: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding product: {ex.Message}");
            }
        }

        private async Task ChooseImage()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Pick an image"
            });

            if (result != null)
            {
                _imageStream = await result.OpenReadAsync();
                _imageName = result.FileName;
            }
        }


        private void OnAddToCart(Product product)
        {
            // Add product to the cart
        }
    }
}

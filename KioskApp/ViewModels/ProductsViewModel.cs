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
        private Stream _imageStream;
        private string _imageName;

        public ProductsViewModel()
        {
            _apiService = DependencyService.Get<IApiService>();
            Initialize();
        }

        public ProductsViewModel(IApiService apiService)
        {
            _apiService = apiService;
            Initialize();
        }

        private void Initialize()
        {
            Products = new ObservableCollection<Product>();
            NewProduct = new Product();
            LoadProductsCommand = new Command(async () => await LoadProducts());
            AddProductCommand = new Command(async () => await AddProduct());
            ChooseImageCommand = new Command(async () => await ChooseImage());
            AddToCartCommand = new Command<Product>(OnAddToCart);

            LoadProductsCommand.Execute(null);
        }

        public ObservableCollection<Product> Products { get; private set; }
        public Product NewProduct { get; set; }
        public ICommand LoadProductsCommand { get; private set; }
        public ICommand AddProductCommand { get; private set; }
        public ICommand ChooseImageCommand { get; private set; }
        public ICommand AddToCartCommand { get; private set; }

        private async Task LoadProducts()
        {
            var products = await _apiService.GetProducts();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
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


        private void OnAddToCart(Product product)
        {
            // Add product to the cart
        }
    }
}

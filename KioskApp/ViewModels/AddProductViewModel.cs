using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace KioskApp.ViewModels
{
    public class AddProductViewModel : BaseViewModel
    {
        private readonly IProductApiService _productApiService;
        private Stream _imageStream;
        private string _imageName;
        private string _imagePath;
        private string _errorMessage;

        public AddProductViewModel()
        {
            _productApiService = DependencyService.Get<IProductApiService>();
            NewProduct = new Product();
            ChooseImageCommand = new Command(async () => await ChooseImage());
            AddProductCommand = new Command(async () => await AddProduct());
        }

        public Product NewProduct { get; set; }

        public ICommand ChooseImageCommand { get; }
        public ICommand AddProductCommand { get; }

        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }


        private async Task AddProduct()
        {
            try
            {
                if (!Validate()) return;

                NewProduct.ImageUrl = "";

                await _productApiService.AddProduct(NewProduct, _imageStream, _imageName);

                NewProduct = new Product(); // Очистка формы после добавления продукта
                _imageStream?.Dispose(); // Закрытие потока после использования
                _imageStream = null; // Сброс переменной после использования

                await Shell.Current.GoToAsync(".."); // Navigate back to the previous page

                MessagingCenter.Send(this, "ProductAdded");
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
                ImagePath = result.FullPath;
            }
        }


        private bool Validate()
        {
            if (string.IsNullOrEmpty(NewProduct.Name) ||
                string.IsNullOrEmpty(NewProduct.Description) ||
                string.IsNullOrEmpty(NewProduct.Category) ||
                !NewProduct.Price.HasValue ||
                !NewProduct.Stock.HasValue)
            {
                ErrorMessage = "All fields are required.";
                return false;
            }

            if (_imageStream == null)
            {
                ErrorMessage = "Product image is required.";
                return false;
            }

            if (NewProduct.Price <= 0)
            {
                ErrorMessage = "Price must be greater than zero.";
                return false;
            }

            if (NewProduct.Stock < 0)
            {
                ErrorMessage = "Stock cannot be negative.";
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }


    }
}

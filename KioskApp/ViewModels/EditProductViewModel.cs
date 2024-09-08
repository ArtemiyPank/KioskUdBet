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
    [QueryProperty(nameof(Product), "Product")]
    public class EditProductViewModel : BaseViewModel
    {
        private readonly IProductApiService _productApiService;
        private Stream _imageStream;
        private string _imageName;
        private string _imagePath;
        private string _errorMessage;

        public EditProductViewModel()
        {
            _productApiService = DependencyService.Get<IProductApiService>();
            ChooseImageCommand = new Command(async () => await ChooseImage());
            UpdateProductCommand = new Command(async () => await UpdateProduct());
        }

        private Product product;
        public Product Product
        {
            get => product;
            set
            {
                product = value;
                OnPropertyChanged();
                LoadProductDetails(value);
            }
        }

        public ICommand ChooseImageCommand { get; }
        public ICommand UpdateProductCommand { get; }

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

        private void LoadProductDetails(Product product)
        {
            if (product != null)
            {
                ImagePath = product.ImageUrl; // Assuming the URL can be used directly
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

        private async Task UpdateProduct()
        {
            if (!Validate())
            {
                return;
            }

            try
            {
                product.LastUpdated = DateTime.Now;

                await _productApiService.UpdateProduct(Product, _imageStream, _imageName);
                _imageStream?.Dispose(); // Закрытие потока после использования
                _imageStream = null; // Сброс переменной после использования

                // Отправка сообщения об обновлении продуктов
                MessagingCenter.Send(this, "ProductUpdated");

                await Shell.Current.GoToAsync(".."); // Navigate back to the previous page
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HttpRequestException: {ex.Message}");
                ErrorMessage = "An error occurred while updating the product.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating product: {ex.Message}");
                ErrorMessage = "An unexpected error occurred.";
            }
        }

        private bool Validate()
        {
            if (string.IsNullOrEmpty(Product.Name) ||
                string.IsNullOrEmpty(Product.Description) ||
                string.IsNullOrEmpty(Product.Category) ||
                !Product.Price.HasValue ||
                !Product.Stock.HasValue)
            {
                ErrorMessage = "All fields are required.";
                return false;
            }

            if (Product.Price <= 0)
            {
                ErrorMessage = "Price must be greater than zero.";
                return false;
            }

            if (Product.Stock < 0)
            {
                ErrorMessage = "Stock cannot be negative.";
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }
    }
}



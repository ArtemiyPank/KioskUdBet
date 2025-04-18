using System.Diagnostics;
using System.Windows.Input;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;

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
            ChooseImageCommand = new Command(async () => await ChooseImageAsync());
            AddProductCommand = new Command(async () => await AddProductAsync());
        }

        // The product being created
        public Product NewProduct { get; set; }

        // Command to pick an image file
        public ICommand ChooseImageCommand { get; }

        // Command to submit the new product
        public ICommand AddProductCommand { get; }

        // Path of the selected image for display in the UI
        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        // Error message to show validation or upload errors
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Validate inputs and call API to add the product
        private async Task AddProductAsync()
        {
            try
            {
                if (!ValidateInputs()) return;

                // Temporarily clear ImageUrl; API will set it
                NewProduct.ImageUrl = string.Empty;

                await _productApiService.AddProductAsync(NewProduct, _imageStream, _imageName);

                // Reset form
                NewProduct = new Product();
                _imageStream?.Dispose();
                _imageStream = null;

                // Navigate back to the previous page
                await Shell.Current.GoToAsync("..");

                // Notify other parts of the app that a product was added
                MessagingCenter.Send(this, "ProductAdded");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP error adding product: {ex.Message}");
                ErrorMessage = "Network error occurred.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error adding product: {ex.Message}");
                ErrorMessage = "An unexpected error occurred.";
            }
        }

        // Launch file picker to choose an image
        private async Task ChooseImageAsync()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Select a product image"
            });

            if (result != null)
            {
                _imageStream = await result.OpenReadAsync();
                _imageName = result.FileName;
                ImagePath = result.FullPath;
            }
        }

        // Ensure all required fields are filled and valid
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(NewProduct.Name) ||
                string.IsNullOrWhiteSpace(NewProduct.Description) ||
                string.IsNullOrWhiteSpace(NewProduct.Category) ||
                !NewProduct.Price.HasValue ||
                !NewProduct.Stock.HasValue)
            {
                ErrorMessage = "All fields must be completed.";
                return false;
            }

            if (_imageStream == null)
            {
                ErrorMessage = "Please select an image.";
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

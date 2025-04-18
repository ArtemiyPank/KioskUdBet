using System.Diagnostics;
using System.Windows.Input;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;

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
            ChooseImageCommand = new Command(async () => await ChooseImageAsync());
            UpdateProductCommand = new Command(async () => await UpdateProductAsync());
        }

        // The product being edited, populated via QueryProperty
        private Product product;
        public Product Product
        {
            get => product;
            set
            {
                product = value;
                OnPropertyChanged();
                LoadProductDetails();
            }
        }

        // Command to open file picker for image selection
        public ICommand ChooseImageCommand { get; }

        // Command to submit product updates
        public ICommand UpdateProductCommand { get; }

        // Path of the selected or existing image for display
        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        // Error message for validation or server errors
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Initialize view state from the loaded product
        private void LoadProductDetails()
        {
            if (Product != null)
            {
                ImagePath = Product.ImageUrl;
            }
        }

        // Let user pick a new image file
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

        // Validate inputs and call API to update the product
        private async Task UpdateProductAsync()
        {
            if (!ValidateInputs()) return;

            try
            {
                Product.LastUpdated = DateTime.UtcNow;

                await _productApiService.UpdateProductAsync(Product, _imageStream, _imageName);

                _imageStream?.Dispose();
                _imageStream = null;

                MessagingCenter.Send(this, "ProductUpdated");
                await Shell.Current.GoToAsync("..");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP error updating product: {ex.Message}");
                ErrorMessage = "Network error occurred.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error updating product: {ex.Message}");
                ErrorMessage = "An unexpected error occurred.";
            }
        }

        // Ensure all required fields are valid before submission
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(Product.Name) ||
                string.IsNullOrWhiteSpace(Product.Description) ||
                string.IsNullOrWhiteSpace(Product.Category) ||
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

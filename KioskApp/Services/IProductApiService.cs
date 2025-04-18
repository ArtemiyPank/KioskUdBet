using KioskApp.Models;

namespace KioskApp.Services
{
    public interface IProductApiService
    {
        // Download an image stream by URL
        Task<Stream> DownloadProductImageAsync(string imageUrl);

        // Retrieve all products from the API
        Task<List<Product>> GetProductsAsync();

        // Retrieve a single product by its ID
        Task<Product> GetProductByIdAsync(int productId);

        // Add a new product with its image
        Task<Product> AddProductAsync(Product product, Stream imageStream, string imageName);

        // Delete an existing product by its ID
        Task<bool> DeleteProductAsync(int productId);

        // Update product data and image
        Task<bool> UpdateProductAsync(Product product, Stream imageStream, string imageName);

        // Toggle visibility of a product
        Task<bool> ToggleVisibilityAsync(int productId);

        // Reserve stock for a product and return updated stock info
        Task<ProductStockResponse> ReserveProductStockAsync(int productId, int quantity);

        // Release reserved stock and return updated stock info
        Task<ProductStockResponse> ReleaseProductStockAsync(int productId, int quantity);

        // Get the current available stock for a product
        Task<int> GetAvailableStockAsync(int productId);

        // Confirm delivery of a product and adjust stock
        Task ConfirmOrderAsync(int productId, int quantity);
    }
}

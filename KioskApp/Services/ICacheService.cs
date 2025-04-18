using KioskApp.Models;

namespace KioskApp.Services
{
    public interface ICacheService
    {
        // Save product data and associated image stream to the cache directory
        Task SaveProductAsync(Product product, Stream imageStream);

        // Retrieve a cached product by its ID, or null if not found
        Task<Product?> GetProductAsync(int productId);

        // Get the file path of a cached product image, or null if missing
        Task<string?> GetProductImagePath(int productId);

        // Remove all files from the cache directory
        void ClearCache();

        // Delete cache entries (JSON and image) for a specific product
        Task DeleteProduct(int productId);

        // List all product IDs that currently have cache entries
        List<int> GetCachedProductIds();

        // Write detailed information about all cached products to the debug log
        Task LogProductCache();

        // Print the cache directory structure for debugging purposes
        void PrintCacheDirectoryStructure(string? directoryPath = null, string indent = "");
    }
}

using System.Diagnostics;
using System.Text.Json;
using KioskApp.Models;

namespace KioskApp.Services
{
    public class CacheService : ICacheService
    {
        // Directory where cache files are stored
        private readonly string _cacheDirectory;

        public CacheService()
        {
            _cacheDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "cache");

            // Create cache directory if it doesn't exist
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        // Expose cache directory path
        public string CacheDirectory => _cacheDirectory;

        // Save product data and image to cache
        public async Task SaveProductAsync(Product product, Stream imageStream)
        {
            try
            {
                // Remove existing images for this product
                var oldImages = Directory.GetFiles(_cacheDirectory, $"product_{product.Id}*.jpg");
                foreach (var file in oldImages)
                {
                    File.Delete(file);
                }

                // Save new image with a timestamped filename
                var imageFileName = $"product_{product.Id}_{DateTime.UtcNow.Ticks}.jpg";
                var imagePath = Path.Combine(_cacheDirectory, imageFileName);
                using var fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write);
                await imageStream.CopyToAsync(fileStream);

                // Update product's image URL to the cache filename
                product.ImageUrl = imageFileName;

                // Serialize product data to JSON and save
                var productFilePath = Path.Combine(_cacheDirectory, $"product_{product.Id}.json");
                var json = JsonSerializer.Serialize(product);
                await File.WriteAllTextAsync(productFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving product {product.Id} to cache: {ex.Message}");
                throw;
            }
        }

        // Retrieve a cached product by ID, or null if not found
        public async Task<Product?> GetProductAsync(int productId)
        {
            try
            {
                var path = Path.Combine(_cacheDirectory, $"product_{productId}.json");
                if (File.Exists(path))
                {
                    var json = await File.ReadAllTextAsync(path);
                    return JsonSerializer.Deserialize<Product>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading product {productId} from cache: {ex.Message}");
                throw;
            }

            return null;
        }

        // Get the full file path for a cached product image, or null if missing
        public async Task<string?> GetProductImagePath(int productId)
        {
            try
            {
                var product = await GetProductAsync(productId);
                if (product == null || string.IsNullOrEmpty(product.ImageUrl))
                {
                    return null;
                }

                var imagePath = Path.Combine(_cacheDirectory, product.ImageUrl);
                return File.Exists(imagePath) ? imagePath : null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving image for product {productId}: {ex.Message}");
                throw;
            }
        }

        // Remove all files from the cache
        public void ClearCache()
        {
            try
            {
                if (Directory.Exists(_cacheDirectory))
                {
                    foreach (var file in Directory.GetFiles(_cacheDirectory))
                    {
                        File.Delete(file);
                    }

                    Debug.WriteLine("Cleared all cache files.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing cache: {ex.Message}");
            }
        }

        // Delete a specific product's cache files (JSON and one JPG)
        public async Task DeleteProduct(int productId)
        {
            try
            {
                var jsonPath = Path.Combine(_cacheDirectory, $"product_{productId}.json");
                if (File.Exists(jsonPath))
                {
                    File.Delete(jsonPath);
                }

                var jpgPath = Path.Combine(_cacheDirectory, $"product_{productId}.jpg");
                if (File.Exists(jpgPath))
                {
                    File.Delete(jpgPath);
                }

                Debug.WriteLine($"Removed product {productId} from cache.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting product {productId} from cache: {ex.Message}");
            }
        }

        // List all cached product IDs based on JSON filenames
        public List<int> GetCachedProductIds()
        {
            return Directory.GetFiles(_cacheDirectory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => name.StartsWith("product_"))
                .Select(name => name["product_".Length..])
                .Select(int.Parse)
                .ToList();
        }

        // Log details of all cached products to the debug output
        public async Task LogProductCache()
        {
            Debug.WriteLine("===== Cached Products =====");
            foreach (var id in GetCachedProductIds())
            {
                var product = await GetProductAsync(id);
                if (product != null)
                {
                    Debug.WriteLine($"ID: {product.Id}");
                    Debug.WriteLine($"Name: {product.Name}");
                    Debug.WriteLine($"Price: {product.Price}");
                    Debug.WriteLine($"Stock: {product.Stock}");
                    Debug.WriteLine($"Reserved: {product.ReservedStock}");
                    Debug.WriteLine($"Image: {product.ImageUrl}");
                    Debug.WriteLine("---------------------------");
                }
            }
            Debug.WriteLine("===== End of Cache =====");
        }

        // Recursively print the cache directory structure for debugging
        public void PrintCacheDirectoryStructure(string? directory = null, string indent = "")
        {
            try
            {
                directory ??= _cacheDirectory;

                // Print files in current directory
                foreach (var file in Directory.GetFiles(directory))
                {
                    Debug.WriteLine($"{indent}├── {Path.GetFileName(file)}");
                }

                // Recurse into subdirectories
                foreach (var dir in Directory.GetDirectories(directory))
                {
                    Debug.WriteLine($"{indent}└── {Path.GetFileName(dir)}");
                    PrintCacheDirectoryStructure(dir, indent + "    ");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error accessing directory {directory}: {ex.Message}");
            }
        }
    }
}

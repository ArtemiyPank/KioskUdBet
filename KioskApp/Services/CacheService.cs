using KioskApp.Models;
using System.Diagnostics;
using System.Text.Json;

namespace KioskApp.Services
{
    public class CacheService : ICacheService
    {
        private readonly string _cacheDirectory;

        public CacheService()
        {
            _cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cache");

            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }
        public string CacheDirectory => _cacheDirectory;

        public async Task SaveProductAsync(Product product, Stream imageStream)
        {
            try
            {
                // Сохранение данных товара
                var productFilePath = Path.Combine(_cacheDirectory, $"{product.Id}.json");
                var productJson = JsonSerializer.Serialize(product);
                await File.WriteAllTextAsync(productFilePath, productJson);

                // Сохранение изображения товара
                var imageFilePath = Path.Combine(_cacheDirectory, $"{product.Id}.jpg");
                using (var fileStream = new FileStream(imageFilePath, FileMode.Create, FileAccess.Write))
                {
                    await imageStream.CopyToAsync(fileStream);
                }

                Debug.WriteLine($"Product {product.Id} cached successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving product {product.Id} to cache: {ex.Message}");
                throw;
            }
        }

        public async Task<Product> GetProductAsync(int productId)
        {
            try
            {
                var productFilePath = Path.Combine(_cacheDirectory, $"{productId}.json");
                if (File.Exists(productFilePath))
                {
                    var productJson = await File.ReadAllTextAsync(productFilePath);
                    return JsonSerializer.Deserialize<Product>(productJson);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting product {productId} from cache: {ex.Message}");
                throw;
            }
            return null;
        }

        public string GetProductImagePath(int productId)
        {
            try
            {
                var imageFilePath = Path.Combine(_cacheDirectory, $"{productId}.jpg");
                return File.Exists(imageFilePath) ? imageFilePath : null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting product image {productId} from cache: {ex.Message}");
                throw;
            }
        }


        public void ClearCache()
        {
            try
            {
                if (Directory.Exists(_cacheDirectory))
                {
                    var files = Directory.GetFiles(_cacheDirectory);
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                    Debug.WriteLine("All cache files deleted.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing cache: {ex.Message}");
            }
        }


        public async Task DeleteProduct(int productId)
        {
            try
            {
                var productFilePath = Path.Combine(_cacheDirectory, $"{productId}.json");
                var imageFilePath = Path.Combine(_cacheDirectory, $"{productId}.jpg");

                if (File.Exists(productFilePath))
                {
                    File.Delete(productFilePath);
                }

                if (File.Exists(imageFilePath))
                {
                    File.Delete(imageFilePath);
                }
                Debug.WriteLine($"Product {productId} removed from cache.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting product {productId} from cache: {ex.Message}");
            }
        }

        public List<int> GetCachedProductIds()
        {
            return Directory.GetFiles(_cacheDirectory, "*.json")
                            .Select(Path.GetFileNameWithoutExtension)
                            .Select(int.Parse)
                            .ToList();
        }


        public async Task LogProductCache()
        {
            Debug.WriteLine("===================================================================================");

            var productIds = GetCachedProductIds();
            foreach (var productId in productIds)
            {
                var product = await GetProductAsync(productId);
                if (product != null)
                {
                    Debug.WriteLine($"Product ID: {product.Id}");
                    Debug.WriteLine($"Name: {product.Name}");
                    Debug.WriteLine($"Description: {product.Description}");
                    Debug.WriteLine($"Price: {product.Price}");
                    Debug.WriteLine($"Stock: {product.Stock}");
                    Debug.WriteLine($"Category: {product.Category}");
                    Debug.WriteLine($"Last Updated: {product.LastUpdated}");
                    Debug.WriteLine($"Image URL: {product.ImageUrl}");

                    Debug.WriteLine("---------------------------------------------------");
                }
            }

            Debug.WriteLine("===================================================================================");
        }
    }
}

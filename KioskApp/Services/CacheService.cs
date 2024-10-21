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
                // Находим все файлы с расширением .jpg, которые начинаются с product_{product.Id}
                var filesToDelete = Directory.GetFiles(_cacheDirectory, $"product_{product.Id}*.jpg");

                foreach (var file in filesToDelete)
                {
                    File.Delete(file);
                }

                // Генерация уникального имени для изображения
                var imageFileName = $"product_{product.Id}_{DateTime.UtcNow.Ticks}.jpg"; // Метка времени добавлена для уникальности
                var imageFilePath = Path.Combine(_cacheDirectory, imageFileName);

                // Сохраняем новое изображение
                using (var fileStream = new FileStream(imageFilePath, FileMode.Create, FileAccess.Write))
                {
                    await imageStream.CopyToAsync(fileStream);
                }

                // Обновляем URL изображения продукта
                product.ImageUrl = imageFileName;

                // Сохранение данных товара
                var productFilePath = Path.Combine(_cacheDirectory, $"product_{product.Id}.json");
                var productJson = JsonSerializer.Serialize(product);
                await File.WriteAllTextAsync(productFilePath, productJson);
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
                var productFilePath = Path.Combine(_cacheDirectory, $"product_{productId}.json");
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

        public async Task<string> GetProductImagePath(int productId)
        {
            try
            {
                var product = await GetProductAsync(productId);

                if (product == null || string.IsNullOrEmpty(product.ImageUrl))
                {
                    return null;
                }

                var imageFilePath = Path.Combine(_cacheDirectory, product.ImageUrl);
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
                var productFilePath = Path.Combine(_cacheDirectory, $"product_{productId}.json");
                var imageFilePath = Path.Combine(_cacheDirectory, $"product_{productId}.jpg");

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
                            .Where(fileName => fileName.StartsWith("product_")) // Фильтруем файлы с префиксом "product_"
                            .Select(fileName => fileName.Replace("product_", "")) // Убираем префикс "product_"
                            .Select(int.Parse) // Преобразуем оставшуюся часть в число
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
                    Debug.WriteLine($"ReservedStock: {product.ReservedStock}");
                    Debug.WriteLine($"Category: {product.Category}");
                    Debug.WriteLine($"Last Updated: {product.LastUpdated}");
                    Debug.WriteLine($"Image URL: {product.ImageUrl}");

                    Debug.WriteLine("---------------------------------------------------");
                }
            }

            Debug.WriteLine("===================================================================================");
        }

        public void PrintCacheDirectoryStructure(string? directoryPath = null, string indent = "")
        {
            try
            {
                if (directoryPath == null) directoryPath = _cacheDirectory;

                var files = Directory.GetFiles(directoryPath);
                var directories = Directory.GetDirectories(directoryPath);

                // Выводим файлы
                foreach (var file in files)
                {
                    Debug.WriteLine($"{indent}├── {Path.GetFileName(file)}");
                }

                // Выводим папки
                foreach (var dir in directories)
                {
                    Debug.WriteLine($"{indent}└── {Path.GetFileName(dir)}");

                    PrintCacheDirectoryStructure(dir, indent + "    ");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error accessing directory {directoryPath}: {ex.Message}");
            }
        }

    }
}

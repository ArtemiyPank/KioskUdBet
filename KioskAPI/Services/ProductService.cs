using KioskAPI.Controllers;
using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KioskAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Product>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> AddProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _context.Products.FindAsync(productId);
        }

        public async Task<Product> UpdateProduct(int id, Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> ToggleVisibility(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            product.IsHidden = !product.IsHidden;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                _logger.LogWarning($"Product with ID {productId} not found.");
                return false;
            }

            // Удаление изображения
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine("wwwroot", product.ImageUrl.TrimStart('/'));
                if (File.Exists(imagePath))
                {
                    try
                    {
                        File.Delete(imagePath);
                        _logger.LogInformation($"Image {imagePath} deleted successfully.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error when deleting an image: {ex.Message}");
                        throw new IOException($"Failed to delete image at {imagePath}", ex);
                    }
                }
                else
                {
                    _logger.LogWarning($"Image file not found at path: {imagePath}");
                }
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Product with ID {productId} deleted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when deleting product with ID {productId}: {ex.Message}");
                throw new Exception($"Failed to delete product with ID {productId}", ex);
            }
        }

        // Резервирование товара
        public async Task<int> ReserveProductStockAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Product not found");

            product.ReserveStock(quantity); // Резервируем товар
            await _context.SaveChangesAsync();
            return product.AvailableStock; // Возвращаем доступное количество
        }

        // Освобождение товара
        public async Task<int> ReleaseProductStockAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Product not found");

            product.ReleaseStock(quantity); // Освобождаем товар
            await _context.SaveChangesAsync();
            return product.AvailableStock; // Возвращаем доступное количество
        }

        // Получение доступного количества товара
        public async Task<int> GetStock(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Product not found");

            return product.Stock;
        }

        // Получение зарезервированного количества товара
        public async Task<int> GetReservedStock(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Product not found");

            return product.ReservedStock;
        }

        // Подтверждение заказа при доставке
        public async Task ConfirmOrderAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Product not found");

            product.ConfirmOrder(quantity); // Обновляем количество на складе
            await _context.SaveChangesAsync();
        }
    }
}

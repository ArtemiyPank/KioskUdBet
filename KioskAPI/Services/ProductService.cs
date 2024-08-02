using KioskAPI.Controllers;
using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
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


        public async Task<bool> UpdateProductAsync(Product product)
        {
            var existingProduct = await _context.Products.FindAsync(product.Id);
            if (existingProduct == null)
            {
                return false;
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.Category = product.Category;
            existingProduct.LastUpdated = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                existingProduct.ImageUrl = product.ImageUrl;
            }

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> HideProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            product.IsHidden = true;
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
    }
}

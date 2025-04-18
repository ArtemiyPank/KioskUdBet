using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            ApplicationDbContext context,
            ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: retrieve all products
        public async Task<List<Product>> GetProductsAsync()
        {
            _logger.LogInformation("Retrieving all products");
            return await _context.Products.ToListAsync();
        }

        // POST: add a new product
        public async Task<Product> AddProductAsync(Product product)
        {
            _logger.LogInformation("Adding product {Name}", product.Name);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {Id} created", product.Id);
            return product;
        }

        // GET: retrieve a product by its ID
        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            _logger.LogInformation("Fetching product {ProductId}", productId);
            return await _context.Products.FindAsync(productId);
        }

        // PUT: update an existing product
        public async Task<Product> UpdateProductAsync(int id, Product product)
        {
            _logger.LogInformation("Updating product {ProductId}", id);

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} updated", id);
            return product;
        }

        // DELETE: remove a product by its ID, including its image file
        public async Task<bool> DeleteProductAsync(int productId)
        {
            _logger.LogInformation("Deleting product {ProductId}", productId);

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", productId);
                return false;
            }

            // Delete image file if present
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine("wwwroot", product.ImageUrl.TrimStart('/'));
                if (File.Exists(imagePath))
                {
                    try
                    {
                        File.Delete(imagePath);
                        _logger.LogInformation("Deleted image at {Path}", imagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete image at {Path}", imagePath);
                        throw;
                    }
                }
                else
                {
                    _logger.LogWarning("Image file not found at {Path}", imagePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} removed from database", productId);
            return true;
        }

        // PUT: toggle product visibility flag
        public async Task<bool> ToggleVisibilityAsync(int id)
        {
            _logger.LogInformation("Toggling visibility for product {ProductId}", id);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", id);
                return false;
            }

            product.IsHidden = !product.IsHidden;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Product {ProductId} visibility set to {IsHidden}",
                id, product.IsHidden);

            return true;
        }

        // POST: reserve stock for a product and return updated available stock
        public async Task<int> ReserveProductStockAsync(int productId, int quantity)
        {
            _logger.LogInformation(
                "Reserving {Quantity} units of product {ProductId}",
                quantity, productId);

            var product = await GetProductByIdAsync(productId)
                ?? throw new KeyNotFoundException("Product not found");

            product.ReserveStock(quantity);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Reserved {Quantity} units; available stock: {Available}",
                quantity, product.AvailableStock);

            return product.AvailableStock;
        }

        // POST: release previously reserved stock and return updated available stock
        public async Task<int> ReleaseProductStockAsync(int productId, int quantity)
        {
            _logger.LogInformation(
                "Releasing {Quantity} units of reserved stock for product {ProductId}",
                quantity, productId);

            var product = await GetProductByIdAsync(productId)
                ?? throw new KeyNotFoundException("Product not found");

            product.ReleaseStock(quantity);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Released {Quantity} units; available stock: {Available}",
                quantity, product.AvailableStock);

            return product.AvailableStock;
        }

        // GET: get total stock level for a product
        public async Task<int> GetStockAsync(int productId)
        {
            _logger.LogInformation("Getting total stock for product {ProductId}", productId);

            var product = await GetProductByIdAsync(productId)
                ?? throw new KeyNotFoundException("Product not found");

            return product.Stock;
        }

        // GET: get reserved stock level for a product
        public async Task<int> GetReservedStockAsync(int productId)
        {
            _logger.LogInformation("Getting reserved stock for product {ProductId}", productId);

            var product = await GetProductByIdAsync(productId)
                ?? throw new KeyNotFoundException("Product not found");

            return product.ReservedStock;
        }

        // POST: confirm an order delivery by deducting from stock
        public async Task ConfirmOrderAsync(int productId, int quantity)
        {
            _logger.LogInformation(
                "Confirming delivery of {Quantity} units for product {ProductId}",
                quantity, productId);

            var product = await GetProductByIdAsync(productId)
                ?? throw new KeyNotFoundException("Product not found");

            product.ConfirmOrder(quantity);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Product {ProductId} stock decremented by {Quantity}",
                productId, quantity);
        }
    }
}

using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
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

            product.IsHidden = true; // Предполагается, что у продукта есть свойство IsHidden
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

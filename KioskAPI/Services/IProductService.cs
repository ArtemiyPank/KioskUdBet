using KioskAPI.Models;

namespace KioskAPI.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProducts();
        Task<Product> AddProduct(Product product);
        Task<Product> GetProductByIdAsync(int productId);
        Task<Product> UpdateProduct(int id, Product product);
        Task<bool> DeleteProductAsync(int productId);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> ToggleVisibility(int id);
    }
}

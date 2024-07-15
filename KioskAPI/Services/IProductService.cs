using KioskAPI.Models;

namespace KioskAPI.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProducts();
        Task<Product> AddProduct(Product product);
        Task<Product> GetProduct(int id);
        Task<Product> UpdateProduct(int id, Product product);
    }
}

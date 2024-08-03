using KioskApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    internal interface IProductApiService
    {
        
        Task<Stream> DownloadProductImage(string imageUrl);

        Task<List<Product>> GetProducts();
        Task<Product> GetProductById(int productId);

        Task<Product> AddProduct(Product product, Stream imageStream, string imageName);
        Task<bool> DeleteProduct(int productId);
        Task<bool> UpdateProduct(Product product, Stream imageStream, string imageName);
        Task<bool> ToggleVisibility(int id);

        Task<Order> PlaceOrder(Order order);
    }
}


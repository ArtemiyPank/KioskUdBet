using KioskApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public interface IProductApiService
    {
        
        Task<Stream> DownloadProductImage(string imageUrl);

        Task<List<Product>> GetProducts();
        Task<Product> GetProductById(int productId);

        Task<Product> AddProduct(Product product, Stream imageStream, string imageName);
        Task<bool> DeleteProduct(int productId);
        Task<bool> UpdateProduct(Product product, Stream imageStream, string imageName);
        Task<bool> ToggleVisibility(int id);

        Task<HttpResponseMessage> ReserveProductStock(int productId, int quantity);
        Task<HttpResponseMessage> ReleaseProductStock(int productId, int quantity);

        Task<Order> PlaceOrder(Order order);
    }
}


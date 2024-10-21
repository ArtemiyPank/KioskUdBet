using KioskApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static KioskApp.Services.ProductApiService;

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

        Task<ProductStockResponse> ReserveProductStock(int productId, int quantity); // Резервирование товара
        Task<ProductStockResponse> ReleaseProductStock(int productId, int quantity); // Освобождение товара
        Task<int> GetAvailableStock(int productId); // Получение доступного количества товара

        Task<Order> PlaceOrder(Order order);

        // Новый метод для подтверждения заказа
        Task ConfirmOrder(int productId, int quantity);
    }
}

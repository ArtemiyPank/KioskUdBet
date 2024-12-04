using KioskAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskAPI.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProducts();
        Task<Product> AddProduct(Product product);
        Task<Product> GetProductByIdAsync(int productId);
        Task<Product> UpdateProduct(int id, Product product);
        Task<bool> DeleteProductAsync(int productId);
        Task<bool> ToggleVisibility(int id);

        Task<int> ReserveProductStockAsync(int productId, int quantity); // Резервирование товара при добавлении в заказ
        Task<int> ReleaseProductStockAsync(int productId, int quantity); // Освобождение товара при отмене или изменении заказа

        Task<int> GetStock(int productId); // Получение доступного количества товара
        Task<int> GetReservedStock(int productId); // Получение зарезервированного количества товара

        Task DeletingDeliveredProducts(int productId, int quantity); // Удаление доставленного товара
        
        Task ConfirmOrderAsync(int productId, int quantity); // Подтверждение заказа при доставке
    }
}

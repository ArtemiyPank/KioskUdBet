using KioskApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public interface IOrderApiService
    {
        Task<Order> PlaceOrder(Order order);
        Task<List<Order>> GetOrders();
        Task<List<Order>> GetActiveOrders();
        Task<Order> GetOrderById(int orderId);
        Task<Order> GetLastOrderOrCreateEmpty(int userId);
        Task<bool> UpdateOrderStatus(int orderId, string status);
        Task<bool> UpdateOrder(Order order);
        Task<string> GetOrderStatus(int orderId);
        Task<bool> CancelOrder(int orderId);

        // Метод для создания пустого заказа
        Task<Order> CreateEmptyOrder(User user);
    }
}

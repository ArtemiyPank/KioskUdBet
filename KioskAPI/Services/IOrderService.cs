using KioskAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskAPI.Services
{
    public interface IOrderService
    {
        Task CreateOrderAsync(Order order); // Создание нового заказа
        Task CreateNewEmptyOrder(User user);
        Task<Order> GetOrderByIdAsync(int id); // Получение заказа по ID
        Task UpdateOrderStatusAsync(int id, string status); // Обновление статуса заказа
        Task DeleteOrderAsync(int id); // Удаление заказа
        Task<Order?> GetLastOrderForUserAsync(int userId); // Получение последнего заказа
        Task<List<Order>> GetAllOrdersAsync(); // Получение всех заказов
        Task<Order> UpdateOrderAsync(Order order); // Обновления заказа
        Task<IEnumerable<OrderItem>> GetOrderItemsForOrderAsync(int orderId); // получение всех элементов звказа
    }
}

using KioskAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskAPI.Services
{
    public interface IOrderService
    {
        Task CreateOrderAsync(Order order);
        Task<Order> GetOrderByIdAsync(int id);
        Task UpdateOrderStatusAsync(int id, string status);
        Task<List<Order>> GetAllOrdersAsync();
    }
}

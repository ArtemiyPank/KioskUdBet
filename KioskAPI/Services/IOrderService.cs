using KioskAPI.Models;

namespace KioskAPI.Services
{
    public interface IOrderService
    {
        // Create a new order record
        Task CreateOrderAsync(Order order);

        // Create and persist an empty order for the specified user
        Task CreateNewEmptyOrderAsync(User user);

        // Retrieve an order by its identifier
        Task<Order?> GetOrderByIdAsync(int id);

        // Update the status of the order
        Task UpdateOrderStatusAsync(int id, string status);

        // Adjust stock levels for an order that has been delivered
        Task UpdateStockForDeliveredOrderAsync(Order order);

        // Remove an order by its identifier
        Task DeleteOrderAsync(int id);

        // Get the most recent order for a given user, or null if none exists
        Task<Order?> GetLastOrderForUserAsync(int userId);

        // Retrieve all orders
        Task<List<Order>> GetAllOrdersAsync();

        // Update an existing order and return the modified instance
        Task<Order> UpdateOrderAsync(Order order);

        // Get all items associated with a specific order
        Task<IEnumerable<OrderItem>> GetOrderItemsForOrderAsync(int orderId);
    }
}

using KioskApp.Models;

namespace KioskApp.Services
{
    public interface IOrderApiService
    {
        // Place a new order
        Task<Order> PlaceOrderAsync(Order order);

        // Retrieve all orders
        Task<List<Order>> GetOrdersAsync();

        // Retrieve only active orders
        Task<List<Order>> GetActiveOrdersAsync();

        // Get a specific order by its ID
        Task<Order> GetOrderByIdAsync(int orderId);

        // Get the last order for a user, or create a new empty one if none exists
        Task<Order> GetLastOrderOrCreateEmptyAsync(int userId);

        // Update the status of an order
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);

        // Update the details of an existing order
        Task<bool> UpdateOrderAsync(Order order);

        // Get the current status of an order
        Task<string> GetOrderStatusAsync(int orderId);

        // Cancel an order by its ID
        Task<bool> CancelOrderAsync(int orderId);

        // Create a new empty order for the given user
        Task<Order> CreateEmptyOrderAsync(User user);
    }
}

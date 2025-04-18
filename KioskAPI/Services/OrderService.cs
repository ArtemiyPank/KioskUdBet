using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IProductService _productService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IProductService productService,
            ApplicationDbContext context,
            ILogger<OrderService> logger)
        {
            _productService = productService;
            _context = context;
            _logger = logger;
        }

        // Create and persist a new order
        public async Task CreateOrderAsync(Order order)
        {
            _logger.LogInformation("Creating order for user {UserId}", order.UserId);

            // Attach existing user and products to avoid re-insert
            _context.Attach(order.User);
            foreach (var item in order.OrderItems)
            {
                _context.Attach(item.Product);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order {OrderId} created", order.Id);
        }

        // Create a new empty order for the user
        public Task CreateNewEmptyOrderAsync(User user)
        {
            var order = Order.CreateNewEmptyOrder(user);
            return CreateOrderAsync(order);
        }

        // Retrieve an order by ID, including items and user
        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // Update the status of an order
        public async Task UpdateOrderStatusAsync(int id, string status)
        {
            var order = await GetOrderByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for status update", id);
                return;
            }

            order.Status = status;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order {OrderId} status set to {Status}", id, status);
        }

        // Adjust product stock when an order is delivered
        public async Task UpdateStockForDeliveredOrderAsync(Order order)
        {
            foreach (var item in order.OrderItems)
            {
                try
                {
                    _logger.LogInformation(
                        "Confirming {Quantity} units for product {ProductId}",
                        item.Quantity, item.ProductId);

                    await _productService.ConfirmOrderAsync(item.ProductId, item.Quantity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error confirming product {ProductId} for order {OrderId}",
                        item.ProductId, order.Id);
                }
            }
        }

        // Remove an order and its items
        public async Task DeleteOrderAsync(int id)
        {
            var order = await GetOrderByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for deletion", id);
                return;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order {OrderId} deleted", id);
        }

        // Retrieve all orders
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .ToListAsync();
        }

        // Update an existing order's details and items
        public async Task<Order> UpdateOrderAsync(Order order)
        {
            _logger.LogInformation("Updating order {OrderId}", order.Id);

            var existing = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (existing == null)
            {
                _logger.LogWarning("Order {OrderId} not found", order.Id);
                return null!;
            }

            // Remove old items
            foreach (var oldItem in existing.OrderItems.ToList())
            {
                _context.OrderItems.Remove(oldItem);
                _logger.LogInformation(
                    "Removed item {ItemId} from order {OrderId}",
                    oldItem.Id, order.Id);
            }
            await _context.SaveChangesAsync();

            // Update order metadata
            existing.Building = order.Building;
            existing.RoomNumber = order.RoomNumber;
            existing.DeliveryStartTime = order.DeliveryStartTime;
            existing.DeliveryEndTime = order.DeliveryEndTime;
            existing.Status = order.Status;
            existing.CreationTime = DateTime.Now;

            // Add new items
            foreach (var newItem in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(newItem.ProductId);
                if (product != null)
                {
                    var orderItem = new OrderItem(product, newItem.Quantity)
                    {
                        OrderId = existing.Id
                    };
                    existing.OrderItems.Add(orderItem);
                    _logger.LogInformation(
                        "Added item for product {ProductId} to order {OrderId}",
                        newItem.ProductId, existing.Id);
                }
            }

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} updated successfully", existing.Id);
            return existing;
        }

        // Get the latest order for a user
        public async Task<Order?> GetLastOrderForUserAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreationTime)
                .FirstOrDefaultAsync();
        }

        // Retrieve items for a specific order
        public async Task<IEnumerable<OrderItem>> GetOrderItemsForOrderAsync(int orderId)
        {
            return await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Product)
                .ToListAsync();
        }
    }
}

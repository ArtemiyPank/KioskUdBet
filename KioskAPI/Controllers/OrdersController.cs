using KioskAPI.Models;
using KioskAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            IUserService userService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _userService = userService;
            _logger = logger;
        }

        // POST: api/order/createEmptyOrder
        [HttpPost("createEmptyOrder")]
        public async Task<IActionResult> CreateEmptyOrder([FromBody] User user)
        {
            _logger.LogInformation("Creating empty order for user {UserId}", user.Id);

            var order = new Order(user, new List<OrderItem>())
            {
                Status = "Not placed"
            };

            try
            {
                await _orderService.CreateOrderAsync(order);
                _logger.LogInformation("Order created with ID {OrderId}", order.Id);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating empty order for user {UserId}", user.Id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // PUT: api/order/{id}/updateOrder
        [HttpPut("{id}/updateOrder")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
        {
            _logger.LogInformation("Updating order {OrderId} with items: {Items}", id, order.ItemsToString());

            try
            {
                var updatedOrder = await _orderService.UpdateOrderAsync(order);
                if (updatedOrder == null)
                {
                    _logger.LogWarning("Order {OrderId} not found for update", id);
                    return NotFound(new { Message = "Order not found" });
                }

                _logger.LogInformation("Order {OrderId} updated successfully", id);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // PUT: api/order/{id}/cancelOrder
        [HttpPut("{id}/cancelOrder")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            _logger.LogInformation("Cancelling order {OrderId}", id);

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for cancellation", id);
                return NotFound(new { Message = "Order not found" });
            }

            try
            {
                foreach (var item in order.OrderItems)
                {
                    _logger.LogInformation(
                        "Releasing reserved quantity {Quantity} for product {ProductId}",
                        item.Quantity, item.ProductId);
                    item.ReleaseProduct();
                }

                await _orderService.DeleteOrderAsync(id);
                _logger.LogInformation("Order {OrderId} cancelled and deleted", id);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // GET: api/order/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            _logger.LogInformation("Retrieving order {OrderId}", id);

            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", id);
                    return NotFound(new { Message = "Order not found" });
                }

                _logger.LogInformation("Returning order {OrderId}", id);
                return Ok(order);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // GET: api/order/user/{userId}/lastOrder
        [HttpGet("user/{userId}/lastOrder")]
        public async Task<IActionResult> GetOrCreateLastOrder(int userId)
        {
            _logger.LogInformation("Retrieving last order for user {UserId}", userId);

            try
            {
                var order = await _orderService.GetLastOrderForUserAsync(userId);

                if (order == null)
                {
                    _logger.LogInformation(
                        "No existing order for user {UserId}; creating new empty order",
                        userId);

                    var user = await _userService.GetUserByIdAsync(userId);
                    order = Order.CreateNewEmptyOrder(user);
                    await _orderService.CreateOrderAsync(order);
                }

                order.OrderItems = (await _orderService
                    .GetOrderItemsForOrderAsync(order.Id))
                    ?.ToList() ?? new List<OrderItem>();

                order.User = await _userService.GetUserByIdAsync(userId);

                _logger.LogInformation("Returning order {OrderId}", order.Id);
                return Ok(order);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving or creating last order for user {UserId}", userId);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // GET: api/order/getOrderStatus/{orderId}
        [HttpGet("getOrderStatus/{orderId}")]
        public async Task<IActionResult> GetOrderStatus(int orderId)
        {
            _logger.LogInformation("Retrieving status for order {OrderId}", orderId);

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                return NotFound(new { Message = "Order not found" });
            }

            return Ok(order.Status);
        }

        // PUT: api/order/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            _logger.LogInformation("Updating status of order {OrderId} to {Status}", id, status);

            try
            {
                await _orderService.UpdateOrderStatusAsync(id, status);
                _logger.LogInformation("Order {OrderId} status updated to {Status}", id, status);

                if (status == "Delivered")
                {
                    var order = await _orderService.GetOrderByIdAsync(id);
                    await _orderService.UpdateStockForDeliveredOrderAsync(order);
                }

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating status of order {OrderId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // GET: api/order
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            _logger.LogInformation("Retrieving all orders");

            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                _logger.LogInformation("Returning {Count} orders", orders.Count);
                return Ok(orders);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // GET: api/order/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrders()
        {
            _logger.LogInformation("Retrieving active orders (excluding 'Delivered' and 'Not placed')");

            try
            {
                var activeOrders = (await _orderService.GetAllOrdersAsync())
                    .Where(o => !string.Equals(o.Status, "Delivered", System.StringComparison.OrdinalIgnoreCase)
                             && !string.Equals(o.Status, "Not placed", System.StringComparison.OrdinalIgnoreCase))
                    .ToList();

                _logger.LogInformation("Returning {Count} active orders", activeOrders.Count);
                return Ok(activeOrders);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active orders");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
    }
}

using KioskAPI.Models;
using KioskAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace KioskAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, IUserService userService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _userService = userService;
            _logger = logger;
        }

        // POST: api/order/createEmptyOrder
        [HttpPost("createEmptyOrder")]
        public async Task<IActionResult> CreateEmptyOrder([FromBody] User user)
        {
            try
            {
                Console.WriteLine($"Creating empty order for user {user.Id}");
                var order = new Order(user, new List<OrderItem>())
                {
                    Status = "Not placed"
                };

                await _orderService.CreateOrderAsync(order);
                Console.WriteLine($"Order created with ID {order.Id}");
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating empty order: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        //// POST: api/order/placeOrder
        //[HttpPost("placeOrder")]
        //public async Task<IActionResult> CreateOrder([FromBody] Order order)
        //{
        //    if (order == null)
        //    {
        //        Console.WriteLine("Order is null");
        //        return BadRequest(new { Message = "Order is null" });
        //    }

        //    try
        //    {
        //        Console.WriteLine($"Placing order for user {order.UserId}");
        //        foreach (var item in order.OrderItems)
        //        {
        //            Console.WriteLine($"Reserving product {item.ProductId} with quantity {item.Quantity}");
        //            item.ReserveProduct(); // Резервируем товар
        //        }
        //        await _orderService.CreateOrderAsync(order);
        //        Console.WriteLine($"Order placed with ID {order.Id}");
        //        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error placing order: {ex.Message}");
        //        return StatusCode(500, new { Message = "Internal server error" });
        //    }
        //}

        // PUT: api/order/{id}/updateOrder
        [HttpPut("{id}/updateOrder")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
        {
            Console.WriteLine($"Updating order {id}");
            try
            {
                Console.WriteLine(order.ItemsToString());

                var updatedOrder = await _orderService.UpdateOrderAsync(order);
                if (updatedOrder == null)
                {
                    Console.WriteLine($"Order {id} not found");
                    return NotFound(new { Message = "Order not found" });
                }

                Console.WriteLine($"Order {id} updated successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order {id}: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }




        // PUT: api/order/{id}/deliver
        [HttpPut("{id}/deliver")]
        public async Task<IActionResult> DeliverOrder(int id)
        {
            Console.WriteLine($"Delivering order {id}");
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                Console.WriteLine($"Order {id} not found");
                return NotFound(new { Message = "Order not found" });
            }

            try
            {
                order.Status = "Delivered";

                foreach (var item in order.OrderItems)
                {
                    Console.WriteLine($"Confirming product {item.ProductId} with quantity {item.Quantity}");
                    item.Product.ConfirmOrder(item.Quantity); // Обновляем количество на складе
                }

                await _orderService.UpdateOrderStatusAsync(id, order.Status);
                Console.WriteLine($"Order {id} delivered");

                var newOrder = Order.CreateNewEmptyOrder(order.User);
                await _orderService.CreateOrderAsync(newOrder);
                Console.WriteLine($"New empty order created with ID {newOrder.Id} after delivery");

                return Ok(new { NewOrderId = newOrder.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error delivering order {id}: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // PUT: api/order/{id}/cancelOrder
        [HttpPut("{id}/cancelOrder")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            Console.WriteLine($"Cancelling order {id}");
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                Console.WriteLine($"Order {id} not found");
                return NotFound(new { Message = "Order not found" });
            }

            try
            {
                foreach (var item in order.OrderItems)
                {
                    Console.WriteLine($"Releasing product {item.ProductId} with quantity {item.Quantity}");
                    item.ReleaseProduct(); // Освобождаем зарезервированный товар
                }

                await _orderService.DeleteOrderAsync(id);
                Console.WriteLine($"Order {id} cancelled and deleted");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling order {id}: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // GET: api/order/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                Console.WriteLine($"Retrieving order {id}");
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    Console.WriteLine($"Order {id} not found");
                    return NotFound(new { Message = "Order not found" });
                }

                Console.WriteLine($"Returning order {id}");
                return Ok(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving order {id}: {ex.Message}");
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }

        // GET: api/order/user/{userId}/lastOrder
        [HttpGet("user/{userId}/lastOrder")]
        public async Task<IActionResult> GetOrCreateLastOrder(int userId)
        {
            try
            {
                Console.WriteLine($"Retrieving last order for user {userId} in GetOrCreateLastOrder");
                var order = await _orderService.GetLastOrderForUserAsync(userId);
                Console.WriteLine($"Flag 1 ---------------");
                // If no order exists, create a new one
                if (order == null)
                {
                    Console.WriteLine($"No order found for user {userId}. Creating a new one.");
                    var user = await _userService.GetUserById(userId);
                    order = Order.CreateNewEmptyOrder(user);
                    await _orderService.CreateOrderAsync(order);
                }

                Console.WriteLine($"Returning order with ID {order.Id}");

                var orderItems = await _orderService.GetOrderItemsForOrderAsync(order.Id);
                order.OrderItems = orderItems?.ToList() ?? new List<OrderItem>();

                order.User = await _userService.GetUserById(userId);

                var jsonOrder = JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true });
                return Ok(jsonOrder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving or creating order for user {userId}: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }


        [HttpGet("getOrderStatus/{orderId}")]
        public async Task<IActionResult> GetOrderStatus(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(order.Status);
        }

        // PUT: api/order/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            try
            {
                Console.WriteLine($"Updating status of order {id} to {status}");
                await _orderService.UpdateOrderStatusAsync(id, status);
                Console.WriteLine($"Order {id} status updated to {status}");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating status of order {id}: {ex.Message}");
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }

        // GET: api/order
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                Console.WriteLine("Retrieving all orders");
                var orders = await _orderService.GetAllOrdersAsync();
                Console.WriteLine($"Returning {orders.Count} orders");
                return Ok(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all orders: {ex.Message}");
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }
    }


}

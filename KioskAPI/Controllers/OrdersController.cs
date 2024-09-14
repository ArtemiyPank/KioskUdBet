using KioskAPI.Models;
using KioskAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace KioskAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }


        // POST: api/order/placeOrder
        [HttpPost("placeOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            if (order == null)
            {
                _logger.LogWarning("CreateOrder called with a null order.");
                return BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Order is null."
                });
            }

            try
            {
                _logger.LogInformation("Creating order for user {UserId}.", order.UserId);
                await _orderService.CreateOrderAsync(order);
                _logger.LogInformation("Order created with ID {OrderId}.", order.Id);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order for user {UserId}.", order.UserId);
                return StatusCode(500, new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        // GET: api/order/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving order with ID {OrderId}.", id);
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found.", id);
                    return NotFound(new ApiResponse
                    {
                        IsSuccess = false,
                        Message = "Order not found."
                    });
                }

                _logger.LogInformation("Order with ID {OrderId} retrieved successfully.", id);
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving order with ID {OrderId}.", id);
                return StatusCode(500, new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        // PUT: api/order/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            try
            {
                _logger.LogInformation("Updating status of order with ID {OrderId} to {Status}.", id, status);
                await _orderService.UpdateOrderStatusAsync(id, status);
                _logger.LogInformation("Order with ID {OrderId} status updated to {Status}.", id, status);
                SseController.NotifyOrderStatusChanged(id, status);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating status of order with ID {OrderId} to {Status}.", id, status);
                return StatusCode(500, new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
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



        // GET: api/order
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                _logger.LogInformation("Retrieving all orders.");
                var orders = await _orderService.GetAllOrdersAsync();
                _logger.LogInformation("{OrderCount} orders retrieved successfully.", orders.Count);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all orders.");
                return StatusCode(500, new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }
    }
}

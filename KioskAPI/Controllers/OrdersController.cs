using KioskAPI.Models;
using KioskAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace KioskAPI.Controllers
{

    public class TestData()
    {
        public string str { get; set; }
        public int num { get; set; }
        public List<int> lst { get; set; }
        public User User { get; set; }



        public override string ToString()
        {
            string data = 
                 $"\nstr - {str} \n" +
                   $"num - {num} \n";

            data += "data - [";
            foreach (var item in lst)
            {
                data += $"{item} ";
            }
            data += "]\n";

            data += $"-----User----- \n{User.ToString()} \n";
            return data;
        }
    }

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


        [HttpPost("testRequest")]
        public async void testRequest([FromBody] TestData data)
        {
            _logger.LogInformation("TestStart");

            _logger.LogInformation(data.ToString());
            
            _logger.LogInformation("TestEnd");
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

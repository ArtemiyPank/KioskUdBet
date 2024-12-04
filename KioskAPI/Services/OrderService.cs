using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IProductService _productService;

        private readonly ApplicationDbContext _context;

        public OrderService(IProductService productService, ApplicationDbContext context)
        {
            _productService = productService;
            _context = context;
        }

        public async Task CreateOrderAsync(Order order)
        {
            // Присоединяем пользователя и продукты к контексту
            _context.Attach(order.User);
            foreach (var orderItem in order.OrderItems)
            {
                _context.Attach(orderItem.Product);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task UpdateOrderStatusAsync(int id, string status)
        {
            var order = await GetOrderByIdAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await GetOrderByIdAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            var orderId = order.Id;
            Console.WriteLine($"Starting UpdateOrderAsync for Order ID: {orderId}");

            // Загружаем существующий заказ из базы данных
            var existingOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (existingOrder == null)
            {
                Console.WriteLine($"Order ID {orderId} not found.");
                return null; // Возвращаем null, если заказ не найден
            }

            // Удаляем все старые элементы заказа
            foreach (var item in existingOrder.OrderItems.ToList())
            {
                _context.OrderItems.Remove(item);
                Console.WriteLine($"Removed existing OrderItem for Product ID: {item.ProductId}");
            }

            await _context.SaveChangesAsync(); // Сохраняем изменения

            // Обновляем основные данные заказа
            existingOrder.Building = order.Building;
            existingOrder.RoomNumber = order.RoomNumber;
            existingOrder.DeliveryStartTime = order.DeliveryStartTime;
            existingOrder.DeliveryEndTime = order.DeliveryEndTime;
            existingOrder.Status = order.Status;

            // Добавляем новые элементы заказа
            foreach (var newItem in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(newItem.ProductId);
                if (product != null)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = newItem.ProductId,
                        Quantity = newItem.Quantity,
                        Product = product,
                        OrderId = existingOrder.Id
                    };
                    existingOrder.OrderItems.Add(orderItem);
                    Console.WriteLine($"Added new OrderItem for Product ID: {newItem.ProductId}");
                }
            }

            _context.Entry(existingOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Order ID {existingOrder.Id} updated successfully.");
                return existingOrder;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving Order ID {existingOrder.Id}: {ex.Message}");
                throw;
            }
        }


        public async Task UpdatingQuantityForDeliveredOrderAsync(Order order)
        {
            foreach (var item in order.OrderItems)
            {
                try
                {
                    Console.WriteLine($"Updating product {item.ProductId} with delivered quantity {item.Quantity}.");
                    await _productService.DeletingDeliveredProducts(item.ProductId, item.Quantity);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating product {item.ProductId}: {ex.Message}");
                }
            }
        }



        public async Task CreateNewEmptyOrder(User user)
        {
            var newOrder = Order.CreateNewEmptyOrder(user);
            await CreateOrderAsync(newOrder);
        }

        public async Task<Order?> GetLastOrderForUserAsync(int userId)
        {
            var lastOrder = await _context.Orders
                .Where(o => o.User.Id == userId)
                .OrderByDescending(o => o.CreationTime)
                .FirstOrDefaultAsync();

            return lastOrder;
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsForOrderAsync(int orderId)
        {
            var OrderItems = await _context.OrderItems
                                   .Where(oi => oi.OrderId == orderId)
                                   .Include(oi => oi.Product)
                                   .ToListAsync();
            return OrderItems;
        }
    }
}

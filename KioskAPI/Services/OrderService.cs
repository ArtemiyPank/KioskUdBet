using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateOrderAsync(Order order)
        {
            // Присоединяем существующего пользователя к контексту, чтобы избежать ошибки уникального ограничения
            _context.Attach(order.User);

            // Присоединяем существующие продукты к контексту, чтобы избежать ошибки уникального ограничения
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

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems) // Loading order items
                .ThenInclude(oi => oi.Product) // Loading products for each order item
                .Include(o => o.User) // Uploading user information
                .ToListAsync();
        }
    }
}

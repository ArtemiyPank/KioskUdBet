﻿using KioskApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public interface IOrderApiService
    {
        Task<Order> PlaceOrder(Order order);
        Task<List<Order>> GetOrders();
        Task<Order> GetOrderById(int orderId);
        Task<bool> UpdateOrderStatus(Order order);
        Task<bool> UpdateOrder(Order order);
    }
}
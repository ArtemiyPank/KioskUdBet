using KioskApp.Helpers;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KioskApp.Models
{
    public class Order : ObservableObject
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public string Building => User.Building;
        public string RoomNumber => User.RoomNumber;
        public DateTime DeliveryStartTime { get; set; }
        public DateTime DeliveryEndTime { get; set; }

        private string status = "Not placed";
        public string Status
        {
            get => status;
            set => SetProperty(ref status, value);  // This method automatically raises the PropertyChanged event.
        }

        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        // Конструктор для инициализации заказа
        public Order(User user, List<OrderItem> orderItems)
        {
            User = user;
            UserId = user.Id;
            OrderItems = orderItems;
            Status = "Not placed"; // Статус по умолчанию
        }

        // Метод для создания нового пустого заказа после доставки
        public static Order CreateNewEmptyOrder(User user)
        {
            return new Order
            {
                User = user,
                UserId = user.Id,
                OrderItems = new List<OrderItem>(), // Пустой список товаров
                Status = "Not placed"
            };
        }

        public bool UpdateOrderItems(AppState appState)
        {
            if (OrderItems == null) return false;
            foreach (var item in OrderItems)
            {
                if (!item.LoadProductFromAppState(appState)) return false;
            }
            return true;
        }

        public override string ToString()
        {
            string data = $"-----Order {Id}----- \n" +
                          $"User - \n" +
                          $"  {User.Id}\n" +
                          $"  {User.FullName}\n" +
                          $"  {User.Email}\n" +
                          $"CreationTime - {CreationTime}\n" +
                          $"DeliveryLocation - {Building}, Room {RoomNumber}\n" +
                          $"DeliveryTime - {DeliveryStartTime:HH:mm} - {DeliveryEndTime:HH:mm}\n";

            data += "OrderItems - \n";
            foreach (var item in OrderItems)
            {
                data += $"   - {item.Product.Name} * {item.Quantity}\n";
            }

            return data;
        }

        public string ItemsToString()
        {
            string data = $"------- Items of the {Id}th order -------";

            foreach (var item in OrderItems)
            {
                data += item.ToString();
            }

            return data;
        }
    }
}

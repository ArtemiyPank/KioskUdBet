using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KioskApp.Models
{
    public class Order
    {
        private User currentUser;
        private ObservableCollection<OrderItem> cartItems;

        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public string Building { get; set; }
        public string RoomNumber { get; set; }
        public DateTime DeliveryStartTime { get; set; }
        public DateTime DeliveryEndTime { get; set; }
        public string Status { get; set; } = "Placed"; // Статус заказа по умолчанию - "Оформлен"

        // Перечисление возможных статусов заказа
        //public enum OrderStatus
        //{
        //    Placed,    // Оформлен
        //    Assembling, // На сборке
        //    Delivered  // Доставлен
        //}
        //public OrderStatus Status { get; set; } = OrderStatus.Placed; // Статус заказа по умолчанию - "Оформлен"


        public Order() { }


        // Конструктор для инициализации данных заказа
        public Order(User user, List<OrderItem> orderItems)
        {
            User = user;
            UserId = user.Id;
            OrderItems = orderItems;
            Building = user.Building;
            RoomNumber = user.RoomNumber;
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

            data += $"OrderItems - \n";
            foreach (var item in OrderItems)
            {
                data += $"   - {item.Product.Name} * {item.Quantity}\n";
            }

            return data;
        }
    }
}

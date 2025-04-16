using System;
using System.Collections.Generic;

namespace KioskAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public List<OrderItem> OrderItems { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public string Building { get; set; }
        public string RoomNumber { get; set; }
        public DateTime DeliveryStartTime { get; set; }
        public DateTime DeliveryEndTime { get; set; }
        public string Status { get; set; } = "Not placed"; // Изначальный статус

        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        public Order(User user, List<OrderItem> orderItems)
        {
            User = user;
            UserId = user.Id;
            Building = user.Building;
            RoomNumber = user.RoomNumber;
            Status = "Not placed";
            OrderItems = orderItems;
            CreationTime = DateTime.Now;
        }

        // Метод для создания нового пустого заказа со статусом "Not placed"
        public static Order CreateNewEmptyOrder(User user)
        {
            var newOrder = new Order
            {
                User = user,
                UserId = user.Id,
                Building = user.Building,
                RoomNumber = user.RoomNumber,
                Status = "Not placed",
                OrderItems = new List<OrderItem>(),
                CreationTime = DateTime.Now
            };

            Console.WriteLine(newOrder.ToString());

            return newOrder;
        }

        public override string ToString()
        {
            string data = $"Order ID: {Id}\nUser ID: {UserId}\nUser: {User.FirstName} {User.LastName}\n" +
                          $"Building: {Building}, Room: {RoomNumber}\n" +
                          $"Delivery Time: {DeliveryStartTime:HH:mm} - {DeliveryEndTime:HH:mm}\n" +
                          $"Status: {Status}\nOrder Items:\n";

            foreach (var item in OrderItems)
            {
                data += $"- {item.Product.Name} x {item.Quantity}\n";
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

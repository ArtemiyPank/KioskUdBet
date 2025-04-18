using System.Text;
using KioskApp.Helpers;
using MvvmHelpers;

namespace KioskApp.Models
{
    public class Order : ObservableObject
    {
        // Unique identifier for the order
        public int Id { get; set; }

        // Identifier of the user who placed the order
        public int UserId { get; set; }

        // User who placed the order
        public User User { get; set; }

        // Collection of items included in this order
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Time when the order was created
        public DateTime CreationTime { get; set; } = DateTime.Now;

        // Delivery location derived from the user
        public string Building => User.Building;
        public string RoomNumber => User.RoomNumber;

        // Delivery time window
        public DateTime DeliveryStartTime { get; set; }
        public DateTime DeliveryEndTime { get; set; }

        // Current order status (raises PropertyChanged when updated)
        private string status = "Not placed";
        public string Status
        {
            get => status;
            set => SetProperty(ref status, value);
        }

        // Default constructor
        public Order() { }

        // Constructor to initialize with user and items
        public Order(User user, List<OrderItem> orderItems)
        {
            User = user;
            UserId = user.Id;
            OrderItems = orderItems ?? new List<OrderItem>();
            Status = "Not placed";
        }

        // Factory method to create a new empty order for a user
        public static Order CreateNewEmptyOrder(User user)
        {
            return new Order
            {
                User = user,
                UserId = user.Id,
                OrderItems = new List<OrderItem>(),
                Status = "Not placed",
                CreationTime = DateTime.Now
            };
        }

        // Load product details for each item from application state
        public bool UpdateOrderItems(AppState appState)
        {
            if (OrderItems == null)
                return false;

            foreach (var item in OrderItems)
            {
                if (!item.LoadProductFromAppState(appState))
                    return false;
            }
            return true;
        }

        // Detailed string representation of the order
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Order {Id}");
            sb.AppendLine($"User: {User.FullName} ({User.Email})");
            sb.AppendLine($"Created: {CreationTime}");
            sb.AppendLine($"Delivery: {Building}, Room {RoomNumber}");
            sb.AppendLine($"Time Window: {DeliveryStartTime:HH:mm} - {DeliveryEndTime:HH:mm}");
            sb.AppendLine("Items:");
            foreach (var item in OrderItems)
            {
                sb.AppendLine($"  - {item.Product.Name} x {item.Quantity}");
            }
            return sb.ToString();
        }

        // Concise string listing of order items
        public string ItemsToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Items for order {Id}: ");
            foreach (var item in OrderItems)
            {
                sb.Append($"{item.Product.Name}({item.Quantity}); ");
            }
            return sb.ToString();
        }
    }
}

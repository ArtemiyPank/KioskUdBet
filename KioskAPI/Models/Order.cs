namespace KioskAPI.Models
{
    public class Order
    {
        // Primary key
        public int Id { get; set; }

        // Foreign key to User
        public int UserId { get; set; }

        // Navigation property for the user who placed the order
        public User User { get; set; }

        // Collection of items included in this order
        public List<OrderItem> OrderItems { get; set; }

        // Timestamp when the order was created
        public DateTime CreationTime { get; set; } = DateTime.Now;

        // Delivery location details
        public string Building { get; set; }
        public string RoomNumber { get; set; }

        // Delivery time window
        public DateTime DeliveryStartTime { get; set; }
        public DateTime DeliveryEndTime { get; set; }

        // Current status of the order
        public string Status { get; set; } = "Not placed";

        // Default constructor initializes the items list
        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        // Constructor to create an order with a user and initial items
        public Order(User user, List<OrderItem> orderItems)
        {
            User = user;
            UserId = user.Id;
            Building = user.Building;
            RoomNumber = user.RoomNumber;
            Status = "Not placed";
            OrderItems = orderItems ?? new List<OrderItem>();
            CreationTime = DateTime.Now;
        }

        // Factory method to create a new empty order for a given user
        public static Order CreateNewEmptyOrder(User user)
        {
            return new Order
            {
                User = user,
                UserId = user.Id,
                Building = user.Building,
                RoomNumber = user.RoomNumber,
                Status = "Not placed",
                OrderItems = new List<OrderItem>(),
                CreationTime = DateTime.Now
            };
        }

        // Returns a detailed string representation of the order
        public override string ToString()
        {
            var details = $"Order ID: {Id}\n" +
                          $"User ID: {UserId}\n" +
                          $"User: {User.FirstName} {User.LastName}\n" +
                          $"Building: {Building}, Room: {RoomNumber}\n" +
                          $"Delivery: {DeliveryStartTime:HH:mm} - {DeliveryEndTime:HH:mm}\n" +
                          $"Status: {Status}\n" +
                          "Items:\n";

            foreach (var item in OrderItems)
            {
                details += $"- {item.Product.Name} x {item.Quantity}\n";
            }

            return details;
        }

        // Returns a concise string of all items in the order
        public string ItemsToString()
        {
            var result = $"Order {Id} Items:";
            foreach (var item in OrderItems)
            {
                result += $" {item.Product.Name}({item.Quantity});";
            }
            return result;
        }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KioskAPI.Models
{
    public class OrderItem
    {
        // Primary key for the order item
        [Key]
        public int Id { get; set; }

        // Foreign key to the parent order (ignored in JSON)
        [ForeignKey("Order"), JsonIgnore]
        public int OrderId { get; set; }

        // Reference to the product being ordered
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Quantity of the product in this order item
        public int Quantity { get; set; }

        // Calculated total price (not stored in database)
        [NotMapped]
        public decimal TotalPrice => Product.Price * Quantity;

        public OrderItem() { }

        // Construct a new order item for a product and quantity
        public OrderItem(Product product, int quantity)
        {
            Product = product;
            ProductId = product.Id;
            Quantity = quantity;
        }

        // Reserve the specified quantity of the product
        public void ReserveProduct()
        {
            Product.ReserveStock(Quantity);
        }

        // Release the reserved quantity of the product
        public void ReleaseProduct()
        {
            Product.ReleaseStock(Quantity);
        }

        // Return a string representation of this order item
        public override string ToString()
        {
            return $"\nOrderItem {Id}: OrderId={OrderId}, ProductId={ProductId}, Quantity={Quantity}";
        }
    }
}

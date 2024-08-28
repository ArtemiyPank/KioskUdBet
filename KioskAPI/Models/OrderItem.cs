using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KioskAPI.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; } // Связь с Order через внешний ключ

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        [NotMapped] // Убираем из базы данных, т. к. TotalPrice только для расчетов
        public decimal TotalPrice => Product.Price * Quantity;

        public OrderItem() { }

        public OrderItem(Product product, int quantity)
        {
            Product = product;
            ProductId = product.Id;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return $"{Product.Name} x {Quantity}";
        }
    }
}

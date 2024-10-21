using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KioskAPI.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Order"), JsonIgnore]
        public int OrderId { get; set; } // Связь с Order через внешний ключ

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        [NotMapped] // Не сохраняем в базу данных, используем для расчетов
        public decimal TotalPrice => Product.Price * Quantity;

        public OrderItem() { }

        public OrderItem(Product product, int quantity)
        {
            Product = product;
            ProductId = product.Id;
            Quantity = quantity;
        }

        // Резервирование товара при добавлении в заказ
        public void ReserveProduct()
        {
            Product.ReserveStock(Quantity);
        }

        // Освобождение товара при удалении или изменении заказа
        public void ReleaseProduct()
        {
            Product.ReleaseStock(Quantity);
        }

        public override string ToString()
        {
            string data = $"\n----- OrderItem {Id} ----- \n" +
                $"OrderId: {OrderId} \n" +
                $"ProductId: {ProductId} \n" +
                $"Quantity: {Quantity} \n";
            return data;
        }
    }
}

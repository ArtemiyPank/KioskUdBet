namespace KioskAPI.Models
{

    public class QuantityRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }


        public override string ToString()
        {
            string rez = $"ProductId - {ProductId} \n" +
                         $"Quantity - {Quantity}";
            return rez;
        }
    }


    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }

        public int Stock { get; set; } // Общее количество товара
        public int ReservedStock { get; set; } // Зарезервированное количество
        public int AvailableStock => Stock - ReservedStock; // Доступное количество

        public string ImageUrl { get; set; }

        public bool IsHidden { get; set; } = false;
        public DateTime LastUpdated { get; set; }

        // Метод для резервирования товара
        public void ReserveStock(int quantity)
        {
            if (AvailableStock >= quantity)
            {
                ReservedStock += quantity;
            }
            else
            {
                throw new Exception("Not enough stock available");
            }
        }

        // Метод для освобождения товара
        public void ReleaseStock(int quantity)
        {
            if (ReservedStock >= quantity)
            {
                ReservedStock -= quantity;
            }
            else
            {
                throw new Exception("Invalid release quantity");
            }
        }

        // Метод для обновления количества товара после оформления заказа
        public void ConfirmOrder(int quantity)
        {
            if (ReservedStock >= quantity)
            {
                Stock -= quantity; // Уменьшаем общее количество товара на складе
                ReservedStock -= quantity; // Уменьшаем зарезервированное количество
            }
            else
            {
                throw new Exception("Insufficient reserved stock to confirm the order");
            }
        }
    }
}

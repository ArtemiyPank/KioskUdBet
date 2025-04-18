namespace KioskAPI.Models
{
    // Request DTO for reserving or releasing product quantities
    public class QuantityRequest
    {
        // ID of the product to adjust
        public int ProductId { get; set; }

        // Quantity to reserve or release
        public int Quantity { get; set; }

        // Return a simple string representation
        public override string ToString() =>
            $"ProductId: {ProductId}, Quantity: {Quantity}";
    }

    // Represents a product in the kiosk inventory
    public class Product
    {
        // Primary key
        public int Id { get; set; }

        // Product name
        public string Name { get; set; }

        // Product description
        public string Description { get; set; }

        // Category name for grouping
        public string Category { get; set; }

        // Price per unit
        public decimal Price { get; set; }

        // Total stock available in inventory
        public int Stock { get; set; }

        // Quantity currently reserved by orders
        public int ReservedStock { get; set; }

        // Stock available for new orders
        public int AvailableStock => Stock - ReservedStock;

        // URL to the product image
        public string ImageUrl { get; set; }

        // Whether the product is hidden from listings
        public bool IsHidden { get; set; } = false;

        // Timestamp of the last update
        public DateTime LastUpdated { get; set; }

        // Reserve a given quantity, reducing available stock
        public void ReserveStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            if (AvailableStock < quantity)
                throw new InvalidOperationException("Not enough stock available");

            ReservedStock += quantity;
        }

        // Release a previously reserved quantity
        public void ReleaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            if (ReservedStock < quantity)
                throw new InvalidOperationException("Cannot release more than reserved");

            ReservedStock -= quantity;
        }

        // Confirm an order by deducting stock and reserved quantities
        public void ConfirmOrder(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            if (ReservedStock < quantity)
                throw new InvalidOperationException("Insufficient reserved stock to confirm order");

            Stock -= quantity;
            ReservedStock -= quantity;
        }
    }
}

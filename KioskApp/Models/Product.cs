using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using KioskApp.Helpers;

namespace KioskApp.Models
{
    public class Product : INotifyPropertyChanged
    {
        // Unique identifier for the product
        public int Id { get; set; }

        // Display name of the product
        public string Name { get; set; }

        // Detailed description of the product
        public string Description { get; set; }

        // Category for grouping products
        public string Category { get; set; }

        // Unit price (nullable if not set)
        public decimal? Price { get; set; }

        // URL to the product image
        public string ImageUrl { get; set; }

        // Whether the product is hidden from listings
        public bool IsHidden { get; set; }

        // Timestamp of the last update
        public DateTime LastUpdated { get; set; }

        private int? _stock;
        // Total stock available in inventory
        public int? Stock
        {
            get => _stock;
            set
            {
                if (_stock != value)
                {
                    _stock = value;
                    OnPropertyChanged(nameof(Stock));
                    OnPropertyChanged(nameof(AvailableStock));
                }
            }
        }

        private int _reservedStock;
        // Quantity currently reserved by orders
        public int ReservedStock
        {
            get => _reservedStock;
            set
            {
                if (_reservedStock != value)
                {
                    _reservedStock = value;
                    OnPropertyChanged(nameof(ReservedStock));
                    OnPropertyChanged(nameof(AvailableStock));
                }
            }
        }

        // Computed available stock = total minus reserved
        public int AvailableStock => (Stock ?? 0) - ReservedStock;

        // Text for a visibility toggle button ("Show" when hidden, "Hide" when visible)
        [JsonIgnore]
        public string VisibilityToggleText => IsHidden ? "Show" : "Hide";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Reserve a specified quantity if stock permits
        public void ReserveStock(int quantity)
        {
            if (DeserializationHelper.IsDeserializing)
            {
                Debug.WriteLine($"Skipping reservation during deserialization: {quantity}");
                return;
            }
            if (quantity <= 0)
            {
                Debug.WriteLine($"Invalid reservation quantity: {quantity}");
                return;
            }
            Debug.WriteLine($"Attempting to reserve {quantity}. Available: {AvailableStock}");
            if (AvailableStock >= quantity)
            {
                ReservedStock += quantity;
                Debug.WriteLine($"Reserved {quantity}. New reserved: {ReservedStock}");
            }
            else
            {
                Debug.WriteLine("Insufficient stock available");
                throw new InvalidOperationException("Not enough stock available");
            }
        }

        // Release a specified quantity of reserved stock
        public void ReleaseStock(int quantity)
        {
            if (DeserializationHelper.IsDeserializing) return;
            if (quantity <= 0)
            {
                Debug.WriteLine($"Invalid release quantity: {quantity}");
                return;
            }
            if (ReservedStock >= quantity)
            {
                ReservedStock -= quantity;
                Debug.WriteLine($"Released {quantity}. New reserved: {ReservedStock}");
            }
            else
            {
                throw new InvalidOperationException("Cannot release more than reserved");
            }
        }

        // Confirm an order by deducting from total and reserved stock
        public void ConfirmOrder(int quantity)
        {
            if (ReservedStock >= quantity)
            {
                Stock -= quantity;
                ReservedStock -= quantity;
                OnPropertyChanged(nameof(Stock));
                OnPropertyChanged(nameof(ReservedStock));
                OnPropertyChanged(nameof(AvailableStock));
                Debug.WriteLine($"Confirmed {quantity}. Stock now: {Stock}, Reserved now: {ReservedStock}");
            }
            else
            {
                throw new InvalidOperationException("Insufficient reserved stock to confirm order");
            }
        }

        // Provide a concise string representation for debugging
        public override string ToString()
        {
            return $"Product {Id}: {Name}\n" +
                   $"Description: {Description}\n" +
                   $"Category: {Category}\n" +
                   $"Price: {Price:C}\n" +
                   $"Stock: {Stock}, Reserved: {ReservedStock}, Available: {AvailableStock}\n" +
                   $"Image URL: {ImageUrl}\n" +
                   $"Is hidden: {IsHidden}\n" +
                   $"Last Updated: {LastUpdated:G}";
        }
    }
}

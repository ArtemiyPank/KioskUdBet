using KioskApp.Helpers;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace KioskApp.Models;

public class Product : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public decimal? Price { get; set; }
    public string ImageUrl { get; set; }
    public bool IsHidden { get; set; } = false;
    public DateTime LastUpdated { get; set; }

    private int? _stock = 0;
    public int? Stock
    {
        get => _stock;
        set
        {
            if (_stock != value)
            {
                _stock = value;
                OnPropertyChanged(nameof(Stock));
            }
        }
    }

    private int _reservedStock;
    public int ReservedStock
    {
        get => _reservedStock;
        set
        {
            if (_reservedStock != value)
            {
                _reservedStock = value;
                OnPropertyChanged(nameof(ReservedStock));
            }
        }
    }
    //public int? Stock { get; set; }
    //public int ReservedStock { get; set; } = 0;
    public int AvailableStock => (Stock ?? 0) - ReservedStock;

    [JsonIgnore]
    public string? VisibilityIsHiddenText => IsHidden ? "Show" : "Hide";

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Метод для резервирования товара
    public void ReserveStock(int quantity)
    {
        if (DeserializationHelper.IsDeserializing) return;
        Debug.WriteLine($"Attempting to reserve {quantity} units. Available stock: {AvailableStock}, Reserved stock: {ReservedStock}, Total stock: {Stock}");

        if (AvailableStock >= quantity)
        {
            ReservedStock += quantity;
            Debug.WriteLine($"Successfully reserved {quantity} units.");
        }
        else
        {
            Debug.WriteLine("Not enough stock available.");
            throw new Exception("Not enough stock available");
        }
    }


    // Метод для освобождения товара
    public void ReleaseStock(int quantity)
    {
        if (DeserializationHelper.IsDeserializing) return;
        if (ReservedStock >= quantity)
        {
            ReservedStock -= quantity;
        }
        else
        {
            throw new Exception("Invalid release quantity");
        }
    }

    // Метод для подтверждения заказа
    public void ConfirmOrder(int quantity)
    {
        if (ReservedStock >= quantity)
        {
            Stock -= quantity;
            ReservedStock -= quantity;
        }
        else
        {
            throw new Exception("Insufficient reserved stock to confirm the order");
        }
    }

    public override string ToString()
    {
        return $"------- Product {Id} ------- \n" +
               $"Name: {Name}\n" +
               $"Description: {Description}\n" +
               $"Category: {Category}\n" +
               $"Price: {Price:C}\n" +
               $"Stock: {Stock}\n" +
               $"Reserved Stock: {ReservedStock}\n" +
               $"Available Stock: {AvailableStock}\n" +
               $"Image URL: {ImageUrl}\n" +
               $"Is hidden: {IsHidden}\n" +
               $"Last Updated: {LastUpdated:G}";
    }
}

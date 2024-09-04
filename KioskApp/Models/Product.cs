using System.Diagnostics;
using System.Text.Json.Serialization;

namespace KioskApp.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
    public string ImageUrl { get; set; }
    public bool IsHidden { get; set; } = false;
    public DateTime LastUpdated { get; set; }

    [JsonIgnore]
    public string? VisibilityIsHiddenText => IsHidden ? "Show" : "Hide";

    public override string ToString()
    {
        return $"Product ID: {Id}\n" +
               $"Name: {Name}\n" +
               $"Description: {Description}\n" +
               $"Category: {Category}\n" +
               $"Price: {Price:C}\n" +
               $"Stock: {Stock}\n" +
               $"Image URL: {ImageUrl}\n" +
               $"Is hidden: {IsHidden}\n" +
               $"Last Updated: {LastUpdated:G}";
    }
}

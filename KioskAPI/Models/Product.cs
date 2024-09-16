namespace KioskAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ReservedStock { get; set; }
        public string ImageUrl { get; set; }
        public bool IsHidden { get; set; } = false;
        public DateTime LastUpdated { get; set; }
    }
}

using System.Collections.ObjectModel;
using System.Diagnostics;
using KioskApp.Models;

namespace KioskApp.Helpers
{
    public class AppState
    {
        // Currently logged-in user
        public User? CurrentUser { get; set; }

        // JWT access token for API calls
        public string? AccessToken { get; set; }

        // Indicates whether the current order has been placed
        public bool IsOrderPlaced { get; set; }

        // Placeholder text for UI testing
        public string TestStr { get; set; } = "Currently empty";

        // Collection of products available in the app
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        // Write all products to debug output
        public void PrintProducts()
        {
            Debug.WriteLine("=== Products in AppState ===");
            foreach (var product in Products)
            {
                Debug.WriteLine(product.ToString());
            }
            Debug.WriteLine("=== End of Products ===");
        }
    }
}

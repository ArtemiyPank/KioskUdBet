using KioskApp.Models;
using KioskApp.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace KioskApp.Helpers
{
    public class AppState
    {
        public User? CurrentUser { get; set; } = null;
        public string? AccessToken { get; set; } = null;
        public bool? IsOrderPlaced { get; set; } = null;

        public string TestStr = "Тут пусто"; 

        public ObservableCollection<Product> Products = new ObservableCollection<Product>();

        //ProductsViewModel _productsViewModel;

        //AppState(ProductsViewModel productsViewModel)
        //{
        //    _productsViewModel = productsViewModel;
        //    _productsViewModel.LoadProducts();
        //}

        public void PrintProducts()
        {
            Debug.WriteLine("==================== Products in AppState ====================");
            foreach (var product in Products)
            {
                Debug.WriteLine(product.ToString());
            }
            Debug.WriteLine("=======================================================================");
        }
    }
}

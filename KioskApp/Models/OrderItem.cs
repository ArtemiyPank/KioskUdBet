using KioskApp.Helpers;
using System.ComponentModel;
using System.Diagnostics;
using KioskApp.Helpers;

namespace KioskApp.Models
{
    public class OrderItem : INotifyPropertyChanged
    {
        //private readonly AppState _appState;
        //public OrderItem(AppState appState)
        //{
        //    _appState = appState;
        //}


        public int Id { get; set; }
        public int ProductId { get; set; }

        //public Product Product { get; set; }

        private Product _product;

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                ProductId = value?.Id ?? 0;
                OnPropertyChanged(nameof(Product));
            }
        }



        public decimal TotalPrice => Product?.Price * Quantity ?? 0;


        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    if (_quantity > value)
                    {
                        Debug.WriteLine($"Releasing stock for Product ID: {ProductId}, Amount: {_quantity - value}");
                        Product.ReleaseStock(_quantity - value); // Освобождаем зарезервированный товар
                    }
                    else
                    {
                        Debug.WriteLine($"Reserving stock for Product ID: {ProductId}, Amount: {value - _quantity}");
                        Product.ReserveStock(value - _quantity); // Резервируем дополнительное количество товара
                    }

                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Product? GetProduct(AppState appState)
        {

            if (LoadProductFromAppState(appState)) return _product;

            Debug.WriteLine($"Load product from AppState was failed");

            return null;
        }


        // Метод для поиска и установки продукта из AppState
        public bool LoadProductFromAppState(AppState appState)
        {
            if (appState == null || appState.Products == null)
            {
                Debug.WriteLine("AppState or Products list is null");
                return false;
            }

            // Ищем продукт в AppState по ProductId
            var product = appState.Products.FirstOrDefault(p => p.Id == ProductId);
            if (product != null)
            {
                Product = product;
                Debug.WriteLine($"Product with ID {ProductId} found and set in OrderItem.");
                return true;
            }
            else
            {
                Debug.WriteLine($"Product with ID {ProductId} not found in AppState.");
                return false;
            }
        }





        public override string ToString()
        {
            string rez = $"------ OrderItem {Id} ------ \n" +
                $"Product: {Product.ToString()}\n" +
                $"Quantity: {Quantity}";

            return rez;
        }

    }
}

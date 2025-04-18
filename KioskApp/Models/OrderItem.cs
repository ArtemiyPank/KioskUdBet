using System.ComponentModel;
using System.Diagnostics;
using KioskApp.Helpers;

namespace KioskApp.Models
{
    // Represents a single item in an order and manages stock adjustments
    public class OrderItem : INotifyPropertyChanged
    {
        // Flag to skip stock operations during initialization or deserialization
        private bool _isInitializing;

        // Unique identifier for this order item
        public int Id { get; set; }

        // Identifier of the associated product
        public int ProductId { get; set; }

        // Determines whether stock changes should be applied when quantity changes
        public bool ShouldManageStock { get; set; }

        private Product _product;
        // The product referenced by this order item
        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                ProductId = value?.Id ?? 0;
                OnPropertyChanged(nameof(Product));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        // Quantity of the product in this order item
        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;

                if (!_isInitializing
                    && !DeserializationHelper.IsDeserializing
                    && ShouldManageStock
                    && Product != null)
                {
                    AdjustStock(value);
                }
                else
                {
                    Debug.WriteLine($"Skipping stock management for ProductId {ProductId}");
                }

                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        // Computed total price based on product price and quantity
        public decimal TotalPrice => Product?.Price * Quantity ?? 0;

        // Default constructor disables stock management until loaded
        public OrderItem()
        {
            ShouldManageStock = false;
        }

        // Adjusts the product stock based on the change in quantity
        private void AdjustStock(int newQuantity)
        {
            var delta = newQuantity - _quantity;
            try
            {
                if (delta > 0)
                {
                    Debug.WriteLine($"Reserving {delta} units of ProductId {ProductId}");
                    Product.ReserveStock(delta);
                }
                else if (delta < 0)
                {
                    Debug.WriteLine($"Releasing {-delta} units of ProductId {ProductId}");
                    Product.ReleaseStock(-delta);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Stock operation failed: {ex.Message}");
                throw;
            }
        }

        // Attempts to load the Product instance from global application state
        public bool LoadProductFromAppState(AppState appState)
        {
            if (appState?.Products == null)
            {
                Debug.WriteLine("AppState or Products collection is null");
                return false;
            }

            var product = appState.Products.FirstOrDefault(p => p.Id == ProductId);
            if (product == null)
            {
                Debug.WriteLine($"ProductId {ProductId} not found in AppState");
                return false;
            }

            Product = product;
            return true;
        }

        // Initializes properties from JSON without triggering stock logic
        public void InitializeFromJson(int productId, int quantity)
        {
            _isInitializing = true;
            try
            {
                ProductId = productId;
                _quantity = quantity;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalPrice));
            }
            finally
            {
                _isInitializing = false;
            }
        }

        // Retrieves the Product after attempting to load it from AppState
        public Product? GetProduct(AppState appState) =>
            LoadProductFromAppState(appState) ? Product : null;

        // Notify UI of property changes
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // String representation for debugging purposes
        public override string ToString() =>
            $"OrderItem {Id}: ProductId={ProductId}, Quantity={Quantity}, TotalPrice={TotalPrice}";
    }
}

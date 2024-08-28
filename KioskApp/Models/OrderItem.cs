using System.ComponentModel;

namespace KioskApp.Models
{
    public class OrderItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public required Product Product { get; set; }

        public decimal TotalPrice => (decimal)(Product.Price * Quantity);

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
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
    }
}

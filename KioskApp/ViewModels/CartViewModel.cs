using System.Collections.ObjectModel;
using System.Windows.Input;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class CartViewModel : BaseViewModel
    {
        private readonly IProductApiService _productApiService;

        public CartViewModel()
        {
            _productApiService = DependencyService.Get<IProductApiService>();
            PlaceOrderCommand = new Command(OnPlaceOrder);
        }

        public string DeliveryLocation { get; set; }
        public ObservableCollection<OrderItem> CartItems { get; set; } = new ObservableCollection<OrderItem>();
        public ICommand PlaceOrderCommand { get; }

        private async void OnPlaceOrder()
        {
            var order = new Order
            {
                UserId = 1, // Placeholder for the current user ID
                OrderItems = CartItems.ToList(),
                OrderDate = DateTime.Now,
                DeliveryLocation = DeliveryLocation
            };

            await _productApiService.PlaceOrder(order);
            // Handle successful order placement
        }
    }
}

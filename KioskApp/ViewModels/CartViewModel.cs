using System;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace KioskApp.ViewModels
{
    public class CartViewModel : BaseViewModel
    {
        private readonly IOrderApiService _orderApiService;
        private readonly IUserService _userService;

        private DateTime _selectedStartTime;
        private DateTime _selectedEndTime;

        public CartViewModel(IOrderApiService orderApiService, IUserService userService)
        {
            _orderApiService = orderApiService ?? throw new ArgumentNullException(nameof(orderApiService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));


            NavigateToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("RegisterPage"));
            PlaceOrderCommand = new Command(OnPlaceOrder);
            AddToCartCommand = new Command<Product>(OnAddToCart);
            IncreaseQuantityCommand = new Command<OrderItem>(OnIncreaseQuantity);
            DecreaseQuantityCommand = new Command<OrderItem>(OnDecreaseQuantity);

            CartItems = new ObservableCollection<OrderItem>();
            LoadCartItems();

            SelectedStartTime = new DateTime(2024, 8, 27, 18, 0, 0);
            SelectedEndTime = new DateTime(2024, 8, 27, 19, 0, 0);
        }

        public string DeliveryLocation => $"{CurrentUser.Building}, Room {CurrentUser.RoomNumber}";

        public DateTime SelectedStartTime
        {
            get => _selectedStartTime;
            set
            {
                _selectedStartTime = value;
                OnPropertyChanged(nameof(SelectedStartTime));
                OnPropertyChanged(nameof(SelectedTimeRangeText));
            }
        }

        public DateTime SelectedEndTime
        {
            get => _selectedEndTime;
            set
            {
                _selectedEndTime = value;
                OnPropertyChanged(nameof(SelectedEndTime));
                OnPropertyChanged(nameof(SelectedTimeRangeText));
            }
        }

        public ObservableCollection<OrderItem> CartItems { get; set; }

        public ICommand NavigateToRegisterCommand { get; }
        public ICommand PlaceOrderCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }

        public User CurrentUser => _userService.GetCurrentUser();
        public bool IsAuthenticated => CurrentUser != null;
        public bool IsNotAuthenticated => !IsAuthenticated;

        public string SelectedTimeRangeText => $"Delivery time: {SelectedStartTime:HH:mm} - {SelectedEndTime:HH:mm}";

        private void LoadCartItems()
        {
            var cartItemsJson = Preferences.Get("CartItems", string.Empty);
            if (!string.IsNullOrEmpty(cartItemsJson))
            {
                var savedCartItems = System.Text.Json.JsonSerializer.Deserialize<List<OrderItem>>(cartItemsJson);
                CartItems = new ObservableCollection<OrderItem>(savedCartItems);
                OnPropertyChanged(nameof(CartItems));
            }
        }

        private void OnAddToCart(Product product)
        {
            var existingItem = CartItems.FirstOrDefault(c => c.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                CartItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 1
                });
            }
            OnPropertyChanged(nameof(CartItems));
            SaveCartItems();
        }

        private void OnIncreaseQuantity(OrderItem item)
        {
            if (item.Product.Stock > item.Quantity)
            {
                item.Quantity++;
            }
            OnPropertyChanged(nameof(CartItems));
            SaveCartItems();
        }

        private void OnDecreaseQuantity(OrderItem item)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
            else
            {
                CartItems.Remove(item);
            }
            OnPropertyChanged(nameof(CartItems));
            SaveCartItems();
        }

        private void SaveCartItems()
        {
            var cartItemsJson = System.Text.Json.JsonSerializer.Serialize(CartItems);
            Preferences.Set("CartItems", cartItemsJson);
        }

        private async void OnPlaceOrder()
        {
            if (!CartItems.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Cart is Empty", "Please add items to your cart before placing an order.", "OK");
                return;
            }

            if (CurrentUser == null)
            {
                Debug.WriteLine("Error: CurrentUser is null.");
                await Application.Current.MainPage.DisplayAlert("Error", "User information is missing. Please log in again.", "OK");
                return;
            }

            var order = new Order(CurrentUser, CartItems.ToList())
            {
                DeliveryStartTime = SelectedStartTime,
                DeliveryEndTime = SelectedEndTime,
            };

            Debug.WriteLine(order.ToString());


            await _orderApiService.PlaceOrder(order);
            await Application.Current.MainPage.DisplayAlert("Order Placed", "Your order has been placed successfully!", "OK");
            CartItems.Clear(); // Clear the cart after successful order
            Preferences.Remove("CartItems"); // Remove cached cart data
        }
    }
}

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
using Syncfusion.Maui.ProgressBar;
using System.Numerics;

namespace KioskApp.ViewModels
{
    public class CartViewModel : BaseViewModel
    {
        private readonly IOrderApiService _orderApiService;
        private readonly IUserService _userService;
        private readonly ISseService _sseService;

        private readonly IUpdateService _updateService;

        private readonly IProductApiService _productApiService;

        private CancellationTokenSource _cts; // токен для остановки мониторинга статуса заказа

        // progressbar
        private ObservableCollection<StepProgressBarItem> _stepProgressItem;
        public ObservableCollection<StepProgressBarItem> StepProgressItem
        {
            get
            {
                return _stepProgressItem;
            }
            set
            {
                _stepProgressItem = value;
            }
        }

        // Progress bar value: 0 - Order not placed, 1 - Placed, 2 - Assembling, 3 - Delivered
        private int _orderStatusValue = 0;
        public int OrderStatusValue
        {
            get => _orderStatusValue;
            set
            {
                Debug.WriteLine("OrderStatusValue was changed");

                _orderStatusValue = value;

                OrderStatusProgress = (value == 3) ? 100 : 40;

                var orderStatusJson = System.Text.Json.JsonSerializer.Serialize(value);
                Preferences.Set("orderStatus", orderStatusJson);

                OnPropertyChanged(nameof(OrderStatusValue));
                OnPropertyChanged(nameof(IsOrderDelivered));
            }
        }

        public bool IsOrderDelivered => (_orderStatusValue == 3);


        private int _orderStatusProgress = 40;
        public int OrderStatusProgress
        {
            get => _orderStatusProgress;
            set
            {
                _orderStatusProgress = value;
                OnPropertyChanged(nameof(OrderStatusProgress));
            }
        }


        private DateTime _selectedStartTime;
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

        private DateTime _selectedEndTime;
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

        private bool _isOrderPlaced = false;
        public bool IsOrderPlaced
        {
            get => _isOrderPlaced;
            set
            {
                _isOrderPlaced = value;

                OnPropertyChanged(nameof(IsOrderPlaced));
                OnPropertyChanged(nameof(IsOrderNotPlaced));
            }
        }
        public bool IsOrderNotPlaced => !IsOrderPlaced;


        public string DeliveryLocation => $"{CurrentUser.Building}, Room {CurrentUser.RoomNumber}";

        private int _orderId;
        public ObservableCollection<OrderItem> CartItems { get; set; }

        public ICommand NavigateToRegisterCommand { get; }
        public ICommand PlaceOrderCommand { get; }
        public ICommand PrepareForTheNextOrderCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }

        public User CurrentUser => _userService.GetCurrentUser();
        public bool IsAuthenticated => CurrentUser != null;
        public bool IsNotAuthenticated => !IsAuthenticated;

        public string SelectedTimeRangeText => $"Delivery time: {SelectedStartTime:HH:mm} - {SelectedEndTime:HH:mm}";



        public CartViewModel(IOrderApiService orderApiService, IUserService userService, ISseService sseService, IProductApiService productApiService, IUpdateService updateService)
        {
            _orderApiService = orderApiService;
            _userService = userService;
            _sseService = sseService;
            _productApiService = productApiService;
            _updateService = updateService;

            NavigateToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("RegisterPage"));
            PlaceOrderCommand = new Command(OnPlaceOrder);
            PrepareForTheNextOrderCommand = new Command(PrepareForTheNextOrder);
            AddToCartCommand = new Command<Product>(OnAddToCart);
            IncreaseQuantityCommand = new Command<OrderItem>(OnIncreaseQuantity);
            DecreaseQuantityCommand = new Command<OrderItem>(OnDecreaseQuantity);

            CartItems = new ObservableCollection<OrderItem>();
            LoadCartItems();


            // range slider lables
            SelectedStartTime = new DateTime(2024, 8, 27, 18, 0, 0);
            SelectedEndTime = new DateTime(2024, 8, 27, 19, 0, 0);

            // progress bar states 
            _stepProgressItem = new ObservableCollection<StepProgressBarItem>();
            _stepProgressItem.Add(new StepProgressBarItem() { PrimaryText = "Placed" });
            _stepProgressItem.Add(new StepProgressBarItem() { PrimaryText = "Assembling" });
            _stepProgressItem.Add(new StepProgressBarItem() { PrimaryText = "Delivered" });


            var orderIdJson = Preferences.Get("OrderId", string.Empty);
            if (!string.IsNullOrEmpty(orderIdJson))
            {
                _orderId = System.Text.Json.JsonSerializer.Deserialize<int>(orderIdJson);
                IsOrderPlaced = true;

                _updateService.StartMonitoringOrderStatus(_orderId, UpdateOrderStatus);

                //StartMonitoringOrderStatus(_orderId);
            }

            var orderStatusJson = Preferences.Get("orderStatus", string.Empty);
            if (!string.IsNullOrEmpty(orderStatusJson))
            {
                OrderStatusValue = System.Text.Json.JsonSerializer.Deserialize<int>(orderStatusJson);
            }
        }


        //// Запуск мониторинга статуса заказа через SSE
        //public void StartMonitoringOrderStatus(int orderId)
        //{
        //    // Инициализация CancellationTokenSource
        //    _cts = new CancellationTokenSource();

        //    // Запуск мониторинга
        //    _sseService.StartMonitoringOrderStatus(orderId, UpdateOrderStatus, _cts.Token);
        //}

        //// Остановка мониторинга статуса заказа
        //public void StopMonitoringOrderStatus()
        //{
        //    if (_cts != null)
        //    {
        //        // Отмена задачи
        //        _cts.Cancel();
        //        _cts.Dispose();
        //        _cts = null;
        //    }
        //}



        private void PrepareForTheNextOrder()
        {
            Debug.WriteLine("In PrepareForTheNextOrder");
            CartItems.Clear();
            OrderStatusValue = 0;
            Preferences.Remove("CartItems");
            Preferences.Remove("OrderId");
            IsOrderPlaced = false;
        }

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

        private async void OnAddToCart(Product product)
        {
            Debug.WriteLine($"_orderStatusValue - {_orderStatusValue}");

            if (_orderStatusValue == 3) PrepareForTheNextOrder();

            var existingItem = CartItems.FirstOrDefault(c => c.ProductId == product.Id);

            // Если товар уже есть в корзине, увеличиваем количество
            if (existingItem != null)
            {
                if (product.Stock - product.ReservedStock > existingItem.Quantity)
                {
                    existingItem.Quantity++;
                    await ReserveProductStockAsync(product.Id, 1); // Резервируем 1 единицу товара
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Stock", "Not enough stock available.", "OK");
                }
            }
            else
            {
                if (product.Stock - product.ReservedStock > 0)
                {
                    CartItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Product = product,
                        Quantity = 1
                    });
                    await ReserveProductStockAsync(product.Id, 1); // Резервируем 1 единицу товара
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Stock", "Not enough stock available.", "OK");
                }
            }

            OnPropertyChanged(nameof(CartItems));
            SaveCartItems();
        }


        private async void OnIncreaseQuantity(OrderItem item)
        {
            if (item.Product.Stock - item.Product.ReservedStock > item.Quantity)
            {
                item.Quantity++;
                await ReserveProductStockAsync(item.Product.Id, 1); // Резервируем 1 единицу товара
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Stock", "Not enough stock available.", "OK");
            }

            OnPropertyChanged(nameof(CartItems));
            SaveCartItems();
        }


        private async void OnDecreaseQuantity(OrderItem item)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
                await ReleaseProductStockAsync(item.Product.Id, 1); // Освобождаем 1 единицу товара
            }
            else
            {
                CartItems.Remove(item);
                await ReleaseProductStockAsync(item.Product.Id, item.Quantity); // Освобождаем все количество товара
            }

            OnPropertyChanged(nameof(CartItems));
            SaveCartItems();
        }

        private async Task ReserveProductStockAsync(int productId, int quantity)
        {
            var response = await _productApiService.ReserveProductStock(productId, quantity);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Failed to reserve stock for product {productId}");
                await Application.Current.MainPage.DisplayAlert("Stock Error", "Failed to reserve stock", "OK");
            }
        }

        private async Task ReleaseProductStockAsync(int productId, int quantity)
        {
            var response = await _productApiService.ReleaseProductStock(productId, quantity);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Failed to release stock for product {productId}");
                await Application.Current.MainPage.DisplayAlert("Stock Error", "Failed to release stock", "OK");
            }
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

            order = await _orderApiService.PlaceOrder(order);
            await Application.Current.MainPage.DisplayAlert("Order Placed", "Your order has been placed successfully!", "OK");

            IsOrderPlaced = true;
            OrderStatusValue = 1; // (Order Placed)
            _orderId = order.Id;

            var orderJson = System.Text.Json.JsonSerializer.Serialize(_orderId);
            Preferences.Set("OrderId", orderJson);

            //// launching update service
            //StartMonitoringOrderStatus(_orderId);
        }


        private void UpdateOrderStatus(string status)
        {
            Debug.WriteLine("In UpdateOrderStatus");
            switch (status)
            {
                case "Placed":
                    OrderStatusValue = 1;
                    break;
                case "Assembling":
                    Debug.WriteLine($"Assembling");
                    OrderStatusValue = 2;
                    break;
                case "Delivered":
                    Debug.WriteLine($"Delivered");
                    OrderStatusValue = 3;

                    _updateService.StopMonitoringOrderStatus();
                    //StopMonitoringOrderStatus(); // Остановка мониторинга статуса заказа после его доставки
                    break;
            }

        }

    }
}

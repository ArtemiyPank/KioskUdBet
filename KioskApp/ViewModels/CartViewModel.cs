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
using System.Reflection;
using KioskApp.Helpers;

namespace KioskApp.ViewModels
{
    public class CartViewModel : BaseViewModel
    {
        private readonly IOrderApiService _orderApiService;
        private readonly IUserService _userService;
        private readonly ISseService _sseService;
        private readonly IUpdateService _updateService;
        private readonly IProductApiService _productApiService;
        private readonly ICacheService _cacheService;
        private readonly AppState _appState;

        private CancellationTokenSource _cts; // токен для остановки мониторинга статуса заказа

        // Прогрессбар для статусов заказа
        private ObservableCollection<StepProgressBarItem> _stepProgressItem;
        public ObservableCollection<StepProgressBarItem> StepProgressItem
        {
            get => _stepProgressItem;
            set => _stepProgressItem = value;
        }

        // Прогресс статуса заказа: 0 - не оформлен, 1 - оформлен, 2 - собирается, 3 - доставлен
        private int _orderStatusValue = 0;
        public int OrderStatusValue
        {
            get => _orderStatusValue;
            set
            {
                //Debug.WriteLine("OrderStatusValue was changed");

                _orderStatusValue = value;
                OrderStatusProgress = (value == 3) ? 100 : 40;

                Preferences.Set("orderStatus", value.ToString());

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

        public Order? _currentOrder;

        public ObservableCollection<OrderItem> CartItems => new ObservableCollection<OrderItem>((_currentOrder ?? new Order()).OrderItems);


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

        public CartViewModel(IOrderApiService orderApiService, IUserService userService, ISseService sseService, IProductApiService productApiService, IUpdateService updateService, ICacheService cacheService, AppState appState)
        {
            try
            {
                Debug.WriteLine($"In CartViewModel");
                _orderApiService = orderApiService;
                _userService = userService;
                _sseService = sseService;
                _productApiService = productApiService;
                _updateService = updateService;
                _cacheService = cacheService;
                _appState = appState;

                NavigateToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("RegisterPage"));
                PlaceOrderCommand = new Command(OnPlaceOrder);
                PrepareForTheNextOrderCommand = new Command(PrepareForTheNextOrder);
                AddToCartCommand = new Command<Product>(OnAddToCart);
                IncreaseQuantityCommand = new Command<OrderItem>(OnIncreaseQuantity);
                DecreaseQuantityCommand = new Command<OrderItem>(OnDecreaseQuantity);


                SelectedStartTime = new DateTime(2024, 8, 27, 18, 0, 0);
                SelectedEndTime = new DateTime(2024, 8, 27, 19, 0, 0);

                _stepProgressItem = new ObservableCollection<StepProgressBarItem>
            {
                new StepProgressBarItem() { PrimaryText = "Placed" },
                new StepProgressBarItem() { PrimaryText = "Assembling" },
                new StepProgressBarItem() { PrimaryText = "Delivered" }
            };
            }
            catch (TargetInvocationException ex)
            {
                Debug.WriteLine($"Error: {ex.InnerException?.Message}");
                Debug.WriteLine($"Stack Trace: {ex.InnerException?.StackTrace}");
            }

            Debug.WriteLine($"Before LoadOrderFromServer()");

            LoadOrderFromServer();

            Debug.WriteLine($"After LoadOrderFromServer()");
        }

        // Загрузка кэшированного заказа
        private async void LoadOrderFromServer()
        {
            Debug.WriteLine($"In LoadOrderFromServer");
            _currentOrder = await _orderApiService.GetLastOrderOrCreateEmpty(CurrentUser.Id);

            foreach (var item in _currentOrder.OrderItems)
            {
                item.Product.ImageUrl = await _cacheService.GetProductImagePath(item.ProductId);
            }

            _updateService.StartMonitoringOrderStatus(_currentOrder.Id, UpdateOrderStatus);
            UpdateOrderStatus(_currentOrder.Status);


            Debug.WriteLine(_currentOrder.ToString());

            Debug.WriteLine("-------CartItems-------");
            foreach (var item in CartItems)
            {
                Debug.WriteLine(item.ToString());
            }


            OnPropertyChanged(nameof(CartItems));


        }

        private async void PrepareForTheNextOrder()
        {
            Debug.WriteLine("In PrepareForTheNextOrder");
            OrderStatusValue = 0;

            IsOrderPlaced = false;

            _updateService.StopMonitoringOrderStatus();

            _currentOrder = await _orderApiService.CreateEmptyOrder(CurrentUser);
            _updateService.StartMonitoringOrderStatus(_currentOrder.Id, UpdateOrderStatus);

            OnPropertyChanged(nameof(CartItems));


        }


        public async void OnAddToCart(Product product)
        {
            Debug.WriteLine($"Attempting to add product {product.Id} to cart.");

            try
            {
                if (!_currentOrder.UpdateOrderItems(_appState))
                {
                    Debug.WriteLine("Updating of OrderItems was failed");

                }


                var existingItem = _currentOrder.OrderItems.FirstOrDefault(c => c.ProductId == product.Id);
                if (existingItem != null)
                {
                    if (product.AvailableStock > existingItem.Quantity)
                    {
                        existingItem.Quantity++;
                        Debug.WriteLine($"Increased quantity of product {product.Id} to {existingItem.Quantity}.");
                        await _productApiService.ReserveProductStock(product.Id, 1);
                        Debug.WriteLine("Stock successfully reserved.");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Stock", "Not enough stock available.", "OK");
                        Debug.WriteLine($"Not enough stock for product {product.Id}.");
                    }
                }
                else
                {
                    if (product.AvailableStock > 0)
                    {
                        var newItem = new OrderItem
                        {
                            ProductId = product.Id,
                            Product = product,
                            Quantity = 1
                        };
                        _currentOrder.OrderItems.Add(newItem);
                        Debug.WriteLine($"Added new product {product.Id} to cart.");
                        await _productApiService.ReserveProductStock(product.Id, 1);
                        Debug.WriteLine("Stock successfully reserved.");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Stock", "Not enough stock available.", "OK");
                        Debug.WriteLine($"Not enough stock for product {product.Id}.");
                    }
                }

                OnPropertyChanged(nameof(CartItems));

                // Обновляем заказ на сервере после добавления товара
                var order = new Order
                {
                    Id = _currentOrder.Id,
                    OrderItems = _currentOrder.OrderItems.ToList(),
                    UserId = CurrentUser.Id,
                    User = CurrentUser
                };
                await _orderApiService.UpdateOrder(order);
                Debug.WriteLine("Order successfully updated on server.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding product {product.Id} to cart: {ex.Message}");
            }
        }


        private async void OnIncreaseQuantity(OrderItem item)
        {
            try
            {
                //await _cacheService.LogProductCache();
                if (!_currentOrder.UpdateOrderItems(_appState))
                {
                    Debug.WriteLine("Updating of OrderItems was failed");

                }   

                Debug.WriteLine($"Item Id: {item.ProductId}");
                Debug.WriteLine($"Increasing quantity for product {item.ProductId}.");
                var orderItem = _currentOrder.OrderItems.FirstOrDefault(c => c.ProductId == item.ProductId);

                Debug.WriteLine($"Before: {orderItem.ToString()}");

                if (orderItem.Product.AvailableStock > item.Quantity)
                {
                    orderItem.Quantity++;
                    await _productApiService.ReserveProductStock(item.ProductId, 1);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Stock", "Not enough stock available.", "OK");
                }

                Debug.WriteLine($"After: {orderItem.ToString()}");
                //Debug.WriteLine(_currentOrder.ItemsToString());


                Debug.WriteLine("Updating order on the server.");
                await _orderApiService.UpdateOrder(_currentOrder);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async void OnDecreaseQuantity(OrderItem item)
        {
            if (!_currentOrder.UpdateOrderItems(_appState))
            {
                Debug.WriteLine("Updating of OrderItems was failed");

            }

            Debug.WriteLine($"Item Id: {item.ProductId}");
            Debug.WriteLine($"Decreasing quantity for product {item.ProductId}.");

            var orderItem = _currentOrder.OrderItems.FirstOrDefault(c => c.ProductId == item.ProductId);
            Debug.WriteLine(orderItem.ToString());

            if (orderItem.Quantity > 1)
            {
                orderItem.Quantity--;
                await _productApiService.ReleaseProductStock(item.ProductId, 1);
            }
            else
            {
                Debug.WriteLine("Remove(orderItem)");
                _currentOrder.OrderItems.Remove(orderItem);
                OnPropertyChanged(nameof(CartItems));

                Debug.WriteLine($"item.ProductId: {item.ProductId}, item.Quantity: {item.Quantity}");
                await _productApiService.ReleaseProductStock(item.ProductId, item.Quantity);
            }
            Debug.WriteLine(orderItem.ToString());
            Debug.WriteLine(_currentOrder.ItemsToString());

            Debug.WriteLine("Updating order on the server.");
            await _orderApiService.UpdateOrder(_currentOrder);

        }


        private async void OnPlaceOrder()
        {
            Debug.WriteLine("Placing order.");

            if (!CartItems.Any())
            {
                Debug.WriteLine("Cart is empty. Cannot place order.");
                await Application.Current.MainPage.DisplayAlert("Cart is Empty", "Please add items to your cart before placing an order.", "OK");
                return;
            }

            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null)
            {
                Debug.WriteLine("User not logged in.");
                await Application.Current.MainPage.DisplayAlert("Error", "User not logged in. Please log in again.", "OK");
                return;
            }

            var order = new Order
            {
                Id = _currentOrder.Id,
                UserId = currentUser.Id,
                User = currentUser,
                OrderItems = CartItems.ToList(),
                CreationTime = DateTime.Now
            };

            await _orderApiService.UpdateOrderStatus(_currentOrder.Id, "Placed");
            UpdateOrderStatus("Placed");
            //await _orderApiService.PlaceOrder(order);
            Debug.WriteLine("Order placed successfully.");
            await Application.Current.MainPage.DisplayAlert("Order Placed", "Your order has been placed successfully!", "OK");

            CartItems.Clear();
        }

        private void UpdateOrderStatus(string status)
        {

            var Cods = new Dictionary<string, string>()
            {
                { "0", "Not placed"},
                { "1", "Placed"},
                { "2", "Assembling"},
                { "3", "Delivered"},
                { "Not placed", "Not placed"},
                { "Placed", "Placed"},
                { "Assembling", "Assembling"},
                { "Delivered", "Delivered"}
            };

            status = Cods[status];

            //Debug.WriteLine($"Updating order status to {status}.");
            switch (status)
            {
                case "Not placed":
                    OrderStatusValue = 0;
                    IsOrderPlaced = false;
                    break;
                case "Placed":
                    OrderStatusValue = 1;
                    IsOrderPlaced = true;
                    break;
                case "Assembling":
                    Debug.WriteLine("Order is assembling.");
                    OrderStatusValue = 2;
                    IsOrderPlaced = true;
                    break;
                case "Delivered":
                    Debug.WriteLine("Order is delivered.");
                    OrderStatusValue = 3;
                    _updateService.StopMonitoringOrderStatus();
                    IsOrderPlaced = true;
                    break;
            }

            //Debug.WriteLine("End of UpdateOrderStatus(string status)");
        }
    }
}

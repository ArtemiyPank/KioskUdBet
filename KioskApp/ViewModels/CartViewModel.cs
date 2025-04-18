using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using KioskApp.Helpers;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;
using Syncfusion.Maui.ProgressBar;

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
        private CancellationTokenSource _cts;

        // Current order loaded from server or cache
        private Order _currentOrder;

        private ObservableCollection<StepProgressBarItem> _stepProgressItem;
        public ObservableCollection<StepProgressBarItem> StepProgressItem
        {
            get => _stepProgressItem;
            set => _stepProgressItem = value;
        }


        public CartViewModel(
            IOrderApiService orderApiService,
            IUserService userService,
            ISseService sseService,
            IProductApiService productApiService,
            IUpdateService updateService,
            ICacheService cacheService,
            AppState appState)
        {
            _orderApiService = orderApiService;
            _userService = userService;
            _sseService = sseService;
            _productApiService = productApiService;
            _updateService = updateService;
            _cacheService = cacheService;
            _appState = appState;

            NavigateToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("RegisterPage"));
            PlaceOrderCommand = new Command(OnPlaceOrderAsync);
            PrepareForTheNextOrderCommand = new Command(PrepareForTheNextOrder);
            AddToCartCommand = new Command<Product>(OnAddToCartAsync);
            IncreaseQuantityCommand = new Command<OrderItem>(OnIncreaseQuantityAsync);
            DecreaseQuantityCommand = new Command<OrderItem>(OnDecreaseQuantityAsync);

            // Initialize delivery time window
            SelectedStartTime = new DateTime(2024, 8, 27, 18, 0, 0);
            SelectedEndTime = new DateTime(2024, 8, 27, 19, 0, 0);


            // Define progress steps
            _stepProgressItem = new ObservableCollection<StepProgressBarItem>
            {
                new StepProgressBarItem() { PrimaryText = "Placed" },
                new StepProgressBarItem() { PrimaryText = "Assembling" },
                new StepProgressBarItem() { PrimaryText = "Delivered" }
            };

            LoadOrderAsync();
        }

        // Commands for UI interactions
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand PlaceOrderCommand { get; }
        public ICommand PrepareForTheNextOrderCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }

        // Progress bar items
        public ObservableCollection<StepProgressBarItem> StepProgressItems { get; private set; }

        // Order status value 0-3, mapped to progress
        private int _orderStatusValue = 0;
        public int OrderStatusValue
        {
            get => _orderStatusValue;
            set
            {
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
            set => SetProperty(ref _orderStatusProgress, value);
        }

        // Delivery time selection
        private DateTime _selectedStartTime;
        public DateTime SelectedStartTime
        {
            get => _selectedStartTime;
            set
            {
                SetProperty(ref _selectedStartTime, value);
                OnPropertyChanged(nameof(SelectedTimeRangeText));
            }
        }

        private DateTime _selectedEndTime;
        public DateTime SelectedEndTime
        {
            get => _selectedEndTime;
            set
            {
                SetProperty(ref _selectedEndTime, value);
                OnPropertyChanged(nameof(SelectedTimeRangeText));
            }
        }

        public string SelectedTimeRangeText =>
            $"Delivery time: {SelectedStartTime:HH:mm} - {SelectedEndTime:HH:mm}";

        // Indicates if the current order has been placed
        private bool _isOrderPlaced;
        public bool IsOrderPlaced
        {
            get => _isOrderPlaced;
            set
            {
                SetProperty(ref _isOrderPlaced, value);
                OnPropertyChanged(nameof(IsOrderNotPlaced));
            }
        }

        public bool IsOrderNotPlaced => !IsOrderPlaced;

        // Current user from application state
        public User CurrentUser => _userService.GetCurrentUser();
        public bool IsAuthenticated => CurrentUser != null;
        public bool IsNotAuthenticated => !IsAuthenticated;

        // Delivery location text
        public string DeliveryLocation =>
            $"{CurrentUser.Building}, Room {CurrentUser.RoomNumber}";

        // Cart items bound to the view
        public ObservableCollection<OrderItem> CartItems =>
            new ObservableCollection<OrderItem>(_currentOrder?.OrderItems ?? new List<OrderItem>());

        // Load or create the last order for the user
        private async void LoadOrderAsync()
        {
            try
            {
                _currentOrder = await _orderApiService.GetLastOrderOrCreateEmptyAsync(CurrentUser.Id);

                // Load cached images for each item
                foreach (var item in _currentOrder.OrderItems)
                {
                    item.Product.ImageUrl = await _cacheService.GetProductImagePath(item.ProductId);
                }

                // Start polling or SSE for status updates
                _updateService.StartMonitoringOrderStatus(_currentOrder.Id, UpdateOrderStatus);
                UpdateOrderStatus(_currentOrder.Status);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading order: {ex.Message}");
            }
            finally
            {
                OnPropertyChanged(nameof(CartItems));
            }
        }

        // Reset for a new empty order
        private async void PrepareForTheNextOrder()
        {
            OrderStatusValue = 0;
            IsOrderPlaced = false;
            _updateService.StopMonitoringOrderStatus();

            _currentOrder = await _orderApiService.CreateEmptyOrderAsync(CurrentUser);
            _updateService.StartMonitoringOrderStatus(_currentOrder.Id, UpdateOrderStatus);
            OnPropertyChanged(nameof(CartItems));
        }

        // Add a product to the cart, reserving stock
        private async void OnAddToCartAsync(Product product)
        {
            try
            {
                var existing = _currentOrder.OrderItems.FirstOrDefault(i => i.ProductId == product.Id);
                if (existing != null)
                {
                    if (product.AvailableStock > existing.Quantity)
                    {
                        existing.Quantity++;
                        await _productApiService.ReserveProductStockAsync(product.Id, 1);
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Stock", "Not enough stock.", "OK");
                    }
                }
                else
                {
                    if (product.AvailableStock > 0)
                    {
                        var newItem = new OrderItem { Product = product, Quantity = 1 };
                        _currentOrder.OrderItems.Add(newItem);
                        await _productApiService.ReserveProductStockAsync(product.Id, 1);
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Stock", "Not enough stock.", "OK");
                    }
                }

                OnPropertyChanged(nameof(CartItems));
                await _orderApiService.UpdateOrderAsync(_currentOrder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding to cart: {ex.Message}");
            }
        }

        // Increase quantity of an item in the cart
        private async void OnIncreaseQuantityAsync(OrderItem item)
        {
            try
            {
                if (item.Product.AvailableStock >= item.Quantity + 1)
                {
                    item.Quantity++;
                    await _productApiService.ReserveProductStockAsync(item.ProductId, 1);
                    await _orderApiService.UpdateOrderAsync(_currentOrder);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Stock", "Not enough stock.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error increasing quantity: {ex.Message}");
            }
        }

        // Decrease quantity or remove item from cart
        private async void OnDecreaseQuantityAsync(OrderItem item)
        {
            try
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    await _productApiService.ReleaseProductStockAsync(item.ProductId, 1);
                }
                else
                {
                    _currentOrder.OrderItems.Remove(item);
                    await _productApiService.ReleaseProductStockAsync(item.ProductId, 1);
                }

                OnPropertyChanged(nameof(CartItems));
                await _orderApiService.UpdateOrderAsync(_currentOrder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error decreasing quantity: {ex.Message}");
            }
        }

        // Place the current order
        private async void OnPlaceOrderAsync()
        {
            if (!CartItems.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Cart Empty", "Add items before placing order.", "OK");
                return;
            }

            await _orderApiService.UpdateOrderStatusAsync(_currentOrder.Id, "Placed");
            UpdateOrderStatus("Placed");

            IsOrderPlaced = true;
            CartItems.Clear();

            await Application.Current.MainPage.DisplayAlert("Order Placed", "Your order has been placed.", "OK");
        }

        // Update local status based on server value
        private void UpdateOrderStatus(string status)
        {
            var mapping = new Dictionary<string, int>
            {
                ["Not placed"] = 0,
                ["Placed"] = 1,
                ["Assembling"] = 2,
                ["Delivered"] = 3
            };

            if (mapping.TryGetValue(status, out var code))
            {
                OrderStatusValue = code;

                IsOrderPlaced = code != 0;

            }
        }
    }
}

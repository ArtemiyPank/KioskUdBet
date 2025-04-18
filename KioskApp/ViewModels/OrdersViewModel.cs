using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using KioskApp.Helpers;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;

namespace KioskApp.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly IOrderApiService _orderApiService;

        // Collection of orders bound to the view
        public ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>();

        // Command to load orders initially
        public ICommand LoadOrdersCommand { get; }
        // Command to manually reload orders
        public ICommand ReloadOrdersCommand { get; }
        // Command to advance an order's status
        public ICommand UpdateOrderStatusCommand { get; }

        public OrdersViewModel(IOrderApiService orderApiService)
        {
            _orderApiService = orderApiService;

            LoadOrdersCommand = new Command(async () => await LoadOrdersAsync());
            ReloadOrdersCommand = new Command(async () => await LoadOrdersAsync());
            UpdateOrderStatusCommand = new Command<Order>(async order => await ExecuteUpdateOrderStatusAsync(order));
            // Automatically load orders when this ViewModel is instantiated
            LoadOrdersCommand.Execute(null);
        }

        // Determine the next status in the workflow
        private string? GetNextStatus(string currentStatus)
        {
            return currentStatus switch
            {
                "Placed" => "Assembling",
                "Assembling" => "Delivered",
                _ => null
            };
        }

        // Handle the UpdateOrderStatusCommand
        private async Task ExecuteUpdateOrderStatusAsync(Order order)
        {
            if (order == null) return;

            var nextStatus = GetNextStatus(order.Status);
            if (string.IsNullOrEmpty(nextStatus)) return;

            await UpdateOrderStatusAsync(order, nextStatus);

            // Remove delivered orders from the list
            if (nextStatus == "Delivered")
            {
                Orders.Remove(order);
            }
        }

        // Load active orders from the API and filter out non-active ones
        public async Task LoadOrdersAsync()
        {
            IsBusy = true;
            DeserializationHelper.IsDeserializing = true;

            try
            {
                var orders = await _orderApiService.GetActiveOrdersAsync();
                Orders.Clear();

                if (orders != null)
                {
                    foreach (var order in orders)
                    {
                        if (order.Status != "Delivered" && order.Status != "Not placed")
                        {
                            Orders.Add(order);
                            foreach (var item in order.OrderItems)
                            {
                                Debug.WriteLine(item.ToString());
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error loading orders: {ex.Message}");
            }
            finally
            {
                DeserializationHelper.IsDeserializing = false;
                IsBusy = false;
            }
        }

        // Call API to update the status, then update the local object
        public async Task UpdateOrderStatusAsync(Order order, string newStatus)
        {
            await _orderApiService.UpdateOrderStatusAsync(order.Id, newStatus);
            order.Status = newStatus;
        }
    }
}

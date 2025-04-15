using KioskApp.Helpers;
using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KioskApp.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly IOrderApiService _orderApiService;
        public ObservableCollection<Order> Orders { get; private set; }

        // Command for initial loading of orders.
        public Command LoadOrdersCommand { get; }
        // Command used for updating the order status.
        public ICommand UpdateOrderStatusCommand { get; }
        // New command for reloading (refreshing) orders manually.
        public Command ReloadOrdersCommand { get; }

        public OrdersViewModel(IOrderApiService orderApiService)
        {
            _orderApiService = orderApiService;
            Orders = new ObservableCollection<Order>();

            // Initialize commands
            LoadOrdersCommand = new Command(async () => await LoadOrdersAsync());
            ReloadOrdersCommand = new Command(async () => await LoadOrdersAsync());
            UpdateOrderStatusCommand = new Command<Order>(async (order) => await ExecuteUpdateOrderStatusCommand(order));

            // Load orders when the ViewModel is created.
            LoadOrdersCommand.Execute(null);
        }

        // Helper method that returns the next order status.
        // Cycle: "Placed" -> "Assembling" -> "Delivered"
        private string GetNextStatus(string currentStatus)
        {
            switch (currentStatus)
            {
                case "Placed":
                    return "Assembling";
                case "Assembling":
                    return "Delivered";
                default:
                    return null;
            }
        }

        // Executes when the UpdateOrderStatusCommand is triggered.
        // It updates the order status and, if the status becomes "Delivered",
        // removes the order from the collection.
        private async Task ExecuteUpdateOrderStatusCommand(Order order)
        {
            if (order == null)
                return;

            string nextStatus = GetNextStatus(order.Status);
            if (string.IsNullOrEmpty(nextStatus))
                return;

            // Call the async method to update the order status via the API.
            await UpdateOrderStatusAsync(order, nextStatus);

            // If the order reaches the final status "Delivered", remove it from the list.
            if (nextStatus == "Delivered")
            {
                Orders.Remove(order);
            }
        }

        // Loads orders from the API and filters them.
        // Only adds orders that are not "Delivered" or "Not placed".
        public async Task LoadOrdersAsync()
        {
            try
            {
                IsBusy = true;

                // Set the global flag to avoid stock operations during deserialization.
                DeserializationHelper.IsDeserializing = true;

                var orders = await _orderApiService.GetOrders();

                Orders.Clear();
                if (orders != null)
                {
                    foreach (var order in orders)
                    {
                        if (order.Status != "Delivered" && order.Status != "Not placed")
                        {
                            Orders.Add(order);

                            foreach(var Item in order.OrderItems)
                            {
                                Console.WriteLine(Item.ToString());

                            }
                            //Console.WriteLine(order.ToString());
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading orders: {ex.Message}");
                // Optionally handle or display the error.
            }
            finally
            {
                // Reset the global flag and busy state.
                DeserializationHelper.IsDeserializing = false;
                IsBusy = false;
            }
        }

        // Calls the API to update the order status and then updates
        // the local order object's Status property.
        public async Task UpdateOrderStatusAsync(Order order, string newStatus)
        {
            await _orderApiService.UpdateOrderStatus(order.Id, newStatus);
            order.Status = newStatus;
        }
    }
}

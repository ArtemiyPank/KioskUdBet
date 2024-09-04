using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace KioskApp.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly IOrderApiService _orderApiService;

        public OrdersViewModel(IOrderApiService orderApiService)
        {
            _orderApiService = orderApiService;
            Orders = new ObservableCollection<Order>();
            LoadOrdersCommand = new Command(async () => await LoadOrdersAsync());
            LoadOrdersCommand.Execute(null);
        }

        public ObservableCollection<Order> Orders { get; private set; }
        public Command LoadOrdersCommand { get; }

        private async Task LoadOrdersAsync()
        {
            var orders = await _orderApiService.GetOrders();
            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
        }

        public async Task UpdateOrderStatusAsync(Order order, string newStatus)
        {
            await _orderApiService.UpdateOrderStatus(order.Id, newStatus);
            order.Status = newStatus;
        }
    }
}

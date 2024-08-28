using KioskApp.Models;
using KioskApp.Services;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System;

namespace KioskApp.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly IOrderApiService _orderApiService;

        public OrdersViewModel()
        {
            _orderApiService = DependencyService.Get<IOrderApiService>();
            Orders = new ObservableCollection<Order>();
            LoadOrdersCommand = new Command(async () => await LoadOrders());

            LoadOrdersCommand.Execute(null);
        }

        public ObservableCollection<Order> Orders { get; private set; }
        public ICommand LoadOrdersCommand { get; private set; }

        private async Task LoadOrders()
        {
            var orders = await _orderApiService.GetOrders();
            Orders.Clear();

            foreach (var order in orders)
            {
                Orders.Add(order);
            }
        }

        public async Task UpdateOrderStatus(Order order, string newStatus)
        {

            order.Status = newStatus;
            await _orderApiService.UpdateOrderStatus(order);
        }
    }
}

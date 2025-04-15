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
        public Command LoadOrdersCommand { get; }
        public ICommand UpdateOrderStatusCommand { get; }

        public OrdersViewModel(IOrderApiService orderApiService)
        {
            _orderApiService = orderApiService;
            Orders = new ObservableCollection<Order>();
            LoadOrdersCommand = new Command(async () => await LoadOrdersAsync());
            LoadOrdersCommand.Execute(null);

            // Создаём команду, которая принимает параметр типа Order.
            UpdateOrderStatusCommand = new Command<Order>(async (order) => await ExecuteUpdateOrderStatusCommand(order));
        }

        // Метод для получения следующего статуса в цепочке: Placed -> Assembling -> Delivered
        private string GetNextStatus(string currentStatus)
        {
            Console.WriteLine($"currentStatus - {currentStatus}");

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

        // Метод выполнения команды обновления статуса заказа
        private async Task ExecuteUpdateOrderStatusCommand(Order order)
        {

            Console.WriteLine("----in ExecuteUpdateOrderStatusCommand ----------");
            if (order == null)
            {
                Console.WriteLine("-----------ORDER IS NULL-----------");
                return;
            }
            // Определяем следующий статус на основе текущего
            string nextStatus = GetNextStatus(order.Status);
            if (string.IsNullOrEmpty(nextStatus))
            {
                Console.WriteLine("-----------STATUS IS NULL-----------");
                return;
            }

            // Вызываем API для обновления статуса и меняем статус заказа
            await UpdateOrderStatusAsync(order, nextStatus);

            // Если заказ получил статус Delivered, удаляем его из списка заказов
            if (nextStatus == "Delivered")
            {
                Orders.Remove(order);
            }
        }

        public async Task LoadOrdersAsync()
        {
            try
            {
                IsBusy = true;

                // Устанавливаем глобальный флаг для предотвращения операций со складом
                DeserializationHelper.IsDeserializing = true;

                var orders = await _orderApiService.GetOrders();

                Orders.Clear();
                if (orders != null)
                {
                    foreach (var order in orders)
                    {
                        if(order.Status != "Delivered" && order.Status != "Not placed")
                            Orders.Add(order);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading orders: {ex.Message}");
                // Обработать ошибку по необходимости
            }
            finally
            {
                // Сбрасываем глобальный флаг
                DeserializationHelper.IsDeserializing = false;
                IsBusy = false;
            }
        }

        // Метод обновления статуса заказа через API
        public async Task UpdateOrderStatusAsync(Order order, string newStatus)
        {
            Console.WriteLine("------------------------IN UpdateOrderStatusAsync------------------------");

            await _orderApiService.UpdateOrderStatus(order.Id, newStatus);
            order.Status = newStatus;
        }
    }
}

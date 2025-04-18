using System.Diagnostics;

namespace KioskApp.Services
{
    // Service that periodically polls the API for updates to a specific order's status
    public class UpdateService : IUpdateService
    {
        private readonly IOrderApiService _orderApiService;
        private Timer _statusUpdateTimer;
        private int _orderId;
        private Action<string> _onStatusUpdate;

        public UpdateService(IOrderApiService orderApiService)
        {
            _orderApiService = orderApiService;
        }

        // Start polling the order status every 5 seconds
        public void StartMonitoringOrderStatus(int orderId, Action<string> onStatusUpdate)
        {
            _orderId = orderId;
            _onStatusUpdate = onStatusUpdate;
            _statusUpdateTimer = new Timer(
                async _ => await CheckOrderStatusAsync(),
                null,
                dueTime: 0,
                period: 5000);
        }

        // Stop polling the order status
        public void StopMonitoringOrderStatus()
        {
            _statusUpdateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _statusUpdateTimer?.Dispose();
        }

        // Fetches the latest status from the API and invokes the update callback
        private async Task CheckOrderStatusAsync()
        {
            try
            {
                var status = await _orderApiService.GetOrderStatusAsync(_orderId);
                if (!string.IsNullOrEmpty(status))
                {
                    _onStatusUpdate?.Invoke(status);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking order status: {ex.Message}");
            }
        }
    }
}

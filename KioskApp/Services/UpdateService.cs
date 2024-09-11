using KioskApp.Services;
using System.Diagnostics;

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

    public void StartMonitoringOrderStatus(int orderId, Action<string> onStatusUpdate)
    {
        _orderId = orderId;
        _onStatusUpdate = onStatusUpdate;
        _statusUpdateTimer = new Timer(async _ => await CheckOrderStatus(), null, 0, 5000); // Проверка каждые 5 секунд
    }

    private async Task CheckOrderStatus()
    {
        try
        {
            var status = await _orderApiService.GetOrderStatus(_orderId);
            if (status != null)
            {   
                _onStatusUpdate?.Invoke(status);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking order status: {ex.Message}");
        }
    }

    public void StopMonitoringOrderStatus()
    {
        _statusUpdateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _statusUpdateTimer?.Dispose();
    }
}
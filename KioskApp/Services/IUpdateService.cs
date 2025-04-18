namespace KioskApp.Services
{
    public interface IUpdateService
    {
        // Start listening for status updates of the specified order
        void StartMonitoringOrderStatus(int orderId, Action<string> onStatusUpdate);

        // Stop listening for order status updates
        void StopMonitoringOrderStatus();
    }
}

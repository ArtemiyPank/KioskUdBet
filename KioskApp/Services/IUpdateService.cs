public interface IUpdateService
{
    void StartMonitoringOrderStatus(int orderId, Action<string> onStatusUpdate);
    void StopMonitoringOrderStatus();
}
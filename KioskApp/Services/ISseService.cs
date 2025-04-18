namespace KioskApp.Services
{
    public interface ISseService
    {
        // Subscribe to stock updates for all products via Server-Sent Events
        // onQuantityUpdate: invoked with (productId, stock, reservedStock)
        Task StartMonitoringAllProductsStockAsync(
            Action<int, int, int> onQuantityUpdate,
            CancellationToken cancellationToken);

        // Alternate method name for starting SSE monitoring
        Task StartAllProductsStockSseAsync(
            Action<int, int, int> onQuantityUpdate,
            CancellationToken cancellationToken);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public interface ISseService
    {
        Task StartMonitoringOrderStatus(int orderId, Action<string> onStatusUpdate, CancellationToken cancellationToken);
        Task StartOrderStatusSseAsync(int orderId, Action<string> onStatusUpdate, CancellationToken cancellationToken);
        Task StartMonitoringOrderStatusProductQuantity(int productId, Action<string> onQuantityUpdate);
        Task StartProductQuantitySseAsync(int productId, Action<string> onQuantityUpdate);
    }
}

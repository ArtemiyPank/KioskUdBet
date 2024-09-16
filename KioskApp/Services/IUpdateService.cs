using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public interface IUpdateService
    {
        void StartMonitoringOrderStatus(int orderId, Action<string> onStatusUpdate);
        void StopMonitoringOrderStatus();
    }
}

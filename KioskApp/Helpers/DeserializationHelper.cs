using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KioskApp.Helpers
{
    public static class DeserializationHelper
    {
        // Static property to track deserialization state
        public static bool IsDeserializing { get; set; } = false;
    }

}

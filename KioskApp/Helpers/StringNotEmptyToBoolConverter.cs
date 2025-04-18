using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace KioskApp.Helpers
{
    // Converts a non-empty string to true, empty or null to false
    public class StringNotEmptyToBoolConverter : IValueConverter
    {
        // Convert string value to boolean
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            return !string.IsNullOrEmpty(str);
        }

        // ConvertBack is not supported
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

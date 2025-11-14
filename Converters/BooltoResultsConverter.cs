using System;
using System.Globalization;
using System.Windows.Data;

namespace LoLTracker.Converters
{
    public class BoolToResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return b ? "Win" : "Loss";
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s) return s.Equals("Win", StringComparison.OrdinalIgnoreCase);
            return false;
        }
    }
}

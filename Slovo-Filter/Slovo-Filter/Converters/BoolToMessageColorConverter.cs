using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Slovo_Filter.Converters
{
    public class BoolToMessageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFromCurrentUser)
            {
                return isFromCurrentUser ? "#0078D7" : "#333333"; // Blue for sent, dark gray for received
            }
            return "#333333"; // Default color if value is null or invalid
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
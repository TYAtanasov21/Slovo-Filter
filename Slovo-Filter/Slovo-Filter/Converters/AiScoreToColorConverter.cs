using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;


    public class AiScoreToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int aiScore && aiScore >= 1)
            {
                return Colors.Red; // Highlight messages with AI Score ≥ 1 in red
            }
            return Colors.DarkGray; // Normal messages stay dark gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
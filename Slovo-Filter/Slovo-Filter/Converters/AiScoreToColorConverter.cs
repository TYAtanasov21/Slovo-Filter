using System.Globalization;

namespace Slovo_Filter.Converters;
public class AiScoreToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int aiScore)
        {
            return aiScore >= 1 ? Colors.Red : Colors.DarkGray;
        }
        return Colors.DarkGray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
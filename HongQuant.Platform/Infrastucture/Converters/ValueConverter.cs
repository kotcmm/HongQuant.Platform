using System.Globalization;
using System.Windows.Data;

namespace HongQuant.Platform.Infrastucture.Converters;
public class ValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var number = (double)value;

        return number.CompareTo(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}

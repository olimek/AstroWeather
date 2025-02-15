using Microsoft.Maui.Controls;
using System;
using System.Globalization;
namespace AstroWeather.Helpers;
public class AstroCondToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double astrocond)
        {
            return astrocond > 50 ? Colors.Green : null;
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

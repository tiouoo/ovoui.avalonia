using System.Globalization;
using Avalonia.Data.Converters;

namespace OvoUi.Common.Converter;

public class BooleansToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool b) return 1.0;
        var isInverted = parameter?.ToString() == "!" || parameter?.ToString() == "Inverse";

        var finalValue = isInverted ? !b : b;

        return finalValue ? 1.0 : 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}
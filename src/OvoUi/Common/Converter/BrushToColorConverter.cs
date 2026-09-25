using System.Globalization;
using Avalonia.Media;

namespace OvoUi.Common.Converter;

public class BrushToColorConverter : MarkupValueConverter
{
    public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ISolidColorBrush b)
        {
            return b.Color;
        }

        return Colors.Transparent;
    }
}
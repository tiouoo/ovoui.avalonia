using System.Globalization;
using Avalonia;

namespace OvoUi.Common.Converter;

public class NavMenuMarginConverter : MarkupMultiValueConverter
{
    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is double indent && values[1] is int level && values[2] is bool b)
        {
            return b ? new Thickness() : new Thickness(indent * (level - 1), 0, 0, 0);
        }

        return AvaloniaProperty.UnsetValue;
    }
}
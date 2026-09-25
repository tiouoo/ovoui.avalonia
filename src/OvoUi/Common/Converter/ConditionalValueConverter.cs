using System.Globalization;
using Avalonia.Data.Converters;

namespace OvoUi.Common.Converter;

public class ConditionalValueConverter : IMultiValueConverter
{
    public static readonly ConditionalValueConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && values[1] is bool isVisible && isVisible)
        {
            return values[0];
        }

        return null;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
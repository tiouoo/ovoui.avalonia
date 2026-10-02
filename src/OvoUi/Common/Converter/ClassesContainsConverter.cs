using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace OvoUi.Common.Converter;

public sealed class ClassesContainsConverter : IValueConverter
{
    public static ClassesContainsConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Classes classes && parameter is string className && classes.Contains(className);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

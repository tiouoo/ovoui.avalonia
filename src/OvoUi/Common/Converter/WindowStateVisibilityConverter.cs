using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace OvoUi.Common.Converter;

public class WindowStateVisibilityConverter : IMultiValueConverter
{
    public static readonly WindowStateVisibilityConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2) return false;

        var isMaxBtnShow = values[0] is bool b && b;
        if (!isMaxBtnShow) return false;

        if (values[1] is not WindowState windowState) return false;

        var showMaximize = parameter?.ToString()?.Equals("Maximize", StringComparison.OrdinalIgnoreCase) == true;

        return showMaximize 
            ? windowState != WindowState.Maximized 
            : windowState == WindowState.Maximized;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
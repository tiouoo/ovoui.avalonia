using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OvoUi.Common.Converter;

public class WindowStateVisibilityConverter : IMultiValueConverter
{
    public static readonly WindowStateVisibilityConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2) return false;

        var isButtonVisible = values[0] is bool b && b;
        if (!isButtonVisible) return false;

        if (values[1] is not WindowState windowState) return false;

        return parameter?.ToString()?.ToUpperInvariant() switch
        {
            "MAXIMIZECONTROL" => windowState != WindowState.FullScreen,
            _ => false
        };
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class WindowControlIconConverter : IMultiValueConverter
{
    public static readonly WindowControlIconConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 3) return null;

        var useActiveIcon = parameter?.ToString()?.ToUpperInvariant() switch
        {
            "MAXIMIZE" => values[0] is WindowState.Maximized,
            "FULLSCREEN" => values[0] is WindowState.FullScreen,
            "PIN" => values[0] is true,
            _ => false
        };

        return values[useActiveIcon ? 2 : 1] is StreamGeometry geometry ? geometry : null;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

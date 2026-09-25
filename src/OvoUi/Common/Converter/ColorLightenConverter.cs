using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OvoUi.Common.Converter;

public class ColorLightenConverter : IValueConverter
{
    public static readonly ColorLightenConverter Lighten15 = new() { Percentage = 0.15 };
    public static readonly ColorLightenConverter Lighten30 = new() { Percentage = 0.30 };
    public static readonly ColorLightenConverter Lighten45 = new() { Percentage = 0.45 };

    public double Percentage { get; set; } = 0.15;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color)
        {
            return LightenColor(color, Percentage);
        }

        if (value is SolidColorBrush brush)
        {
            return new SolidColorBrush(LightenColor(brush.Color, Percentage));
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }

    private static Color LightenColor(Color color, double percentage)
    {
        var r = (byte)Math.Min(255, color.R + (255 - color.R) * percentage);
        var g = (byte)Math.Min(255, color.G + (255 - color.G) * percentage);
        var b = (byte)Math.Min(255, color.B + (255 - color.B) * percentage);
        return Color.FromArgb(color.A, r, g, b);
    }
}
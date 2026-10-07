using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OvoUi.Demo.Controls;

public class VerticalScrollOpacityMaskConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var showTopFade = value is Vector offset && offset.Y > 0.5;
        var stops = new GradientStops();

        if (showTopFade)
        {
            stops.Add(new GradientStop(Colors.Transparent, 0));
            stops.Add(new GradientStop(Colors.White, 0.04));
        }
        else
        {
            stops.Add(new GradientStop(Colors.White, 0));
        }

        stops.Add(new GradientStop(Colors.White, 0.96));
        stops.Add(new GradientStop(Colors.Transparent, 1));

        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops = stops
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

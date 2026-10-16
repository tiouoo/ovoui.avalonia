using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OvoUi.Tab.Converters;

public sealed class HorizontalTabFadeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var showLeftFade = value is Vector offset && offset.X > 0.5;
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops = showLeftFade
                ?
                [
                    new GradientStop(Colors.Transparent, 0),
                    new GradientStop(Colors.White, 0.035),
                    new GradientStop(Colors.White, 0.965),
                    new GradientStop(Colors.Transparent, 1)
                ]
                :
                [
                    new GradientStop(Colors.White, 0),
                    new GradientStop(Colors.White, 0.965),
                    new GradientStop(Colors.Transparent, 1)
                ]
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

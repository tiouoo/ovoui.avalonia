using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OvoUi.Demo.Controls;

public class VerticalScrollOpacityMaskConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var offset = values.Count > 0 && values[0] is double offsetValue ? offsetValue : 0;
        var extent = values.Count > 1 && values[1] is double extentValue ? extentValue : 0;
        var viewport = values.Count > 2 && values[2] is double viewportValue ? viewportValue : 0;
        var hasScrollMetrics = values.Count >= 3 &&
                               values[0] is double &&
                               values[1] is double &&
                               values[2] is double;
        var showTopFade = hasScrollMetrics && offset > 0.5;
        var showBottomFade = hasScrollMetrics && offset + viewport < extent - 0.5;
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

        if (showBottomFade)
        {
            stops.Add(new GradientStop(Colors.White, 0.96));
            stops.Add(new GradientStop(Colors.Transparent, 1));
        }
        else
        {
            stops.Add(new GradientStop(Colors.White, 1));
        }

        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops = stops
        };
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

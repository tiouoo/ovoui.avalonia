using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace OvoUi.Controls;

/// <summary>
/// A scroll viewer that fades content at scrollable vertical edges.
/// </summary>
public class FadingScrollViewer : ScrollViewer
{
    private const double EdgeTolerance = 0.5;
    private const double FadeRatio = 0.04;

    protected override Type StyleKeyOverride => typeof(ScrollViewer);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OffsetProperty ||
            change.Property == ExtentProperty ||
            change.Property == ViewportProperty)
        {
            UpdateOpacityMask();
        }
    }

    private void UpdateOpacityMask()
    {
        var showTopFade = Offset.Y > EdgeTolerance;
        var showBottomFade = Offset.Y + Viewport.Height < Extent.Height - EdgeTolerance;
        var stops = new GradientStops();

        if (showTopFade)
        {
            stops.Add(new GradientStop(Colors.Transparent, 0));
            stops.Add(new GradientStop(Colors.White, FadeRatio));
        }
        else
        {
            stops.Add(new GradientStop(Colors.White, 0));
        }

        if (showBottomFade)
        {
            stops.Add(new GradientStop(Colors.White, 1 - FadeRatio));
            stops.Add(new GradientStop(Colors.Transparent, 1));
        }
        else
        {
            stops.Add(new GradientStop(Colors.White, 1));
        }

        SetCurrentValue(OpacityMaskProperty, new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops = stops
        });
    }
}
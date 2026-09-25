using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace OvoUi.Controls;

public class ContentExpander : ContentControl
{
    public static readonly StyledProperty<double> MultiplierProperty =
        AvaloniaProperty.Register<ContentExpander, double>(nameof(Multiplier));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ContentExpander, Orientation>(nameof(Orientation));

    public static readonly StyledProperty<bool> PreserveDesiredSizeProperty =
        AvaloniaProperty.Register<ContentExpander, bool>(nameof(PreserveDesiredSize));

    static ContentExpander()
    {
        AffectsArrange<ContentExpander>(MultiplierProperty, OrientationProperty);

        AffectsMeasure<ContentExpander>(MultiplierProperty, OrientationProperty);
    }

    public double Multiplier
    {
        get => GetValue(MultiplierProperty);
        set => SetValue(MultiplierProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool PreserveDesiredSize
    {
        get => GetValue(PreserveDesiredSizeProperty);
        set => SetValue(PreserveDesiredSizeProperty, value);
    }

    protected override Size MeasureCore(Size availableSize)
    {
        var result = base.MeasureCore(availableSize);
        return result;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        if (PreserveDesiredSize)
        {
            Clip = new RectangleGeometry(new Rect(0, 0, finalSize.Width, finalSize.Height * Multiplier));
            return result;
        }

        // A very gross way to actually get the bloody thing to draw...
        if (Parent is Control c) c.Margin = new Thickness(1);
        return result;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var result = base.MeasureOverride(availableSize);

        var w = result.Width;
        var h = result.Height;

        if (PreserveDesiredSize) return result;

        switch (Orientation)
        {
            case Orientation.Horizontal:
                w *= Multiplier;
                break;

            case Orientation.Vertical:
                h *= Multiplier;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // A very gross way to actually get the bloody thing to draw...
        if (Parent is Control c) c.Margin = new Thickness(0);
        return new Size(w, h);
    }
}

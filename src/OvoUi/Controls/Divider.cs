using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace OvoUi.Controls;

public class Divider : ContentControl
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Divider, Orientation>(
            nameof(Orientation));

    static Divider()
    {
        HorizontalContentAlignmentProperty.OverrideDefaultValue<Divider>(HorizontalAlignment.Center);
    }

    public Divider()
    {
        HorizontalContentAlignment = HorizontalAlignment.Center;
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}
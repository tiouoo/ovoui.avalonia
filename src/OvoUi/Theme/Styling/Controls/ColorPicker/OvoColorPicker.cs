using Avalonia;

namespace OvoUi.Theme.Styling.Controls.ColorPicker;

public class OvoColorPicker : Avalonia.Controls.ColorPicker
{
    public new static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<OvoColorPicker, int>(nameof(SelectedIndex), 0);
    
    public new int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }
}

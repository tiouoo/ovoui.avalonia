using Avalonia;

namespace OvoUi.Controls;

internal class OvoColorPicker : Avalonia.Controls.ColorPicker
{
    private new static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<OvoColorPicker, int>(nameof(SelectedIndex), 0);
    
    private new int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }
}
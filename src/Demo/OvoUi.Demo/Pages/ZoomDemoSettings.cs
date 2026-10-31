using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using OvoUi.Controls;
using OvoUi.SmoothScroll.Controls;

namespace OvoUi.Demo.Pages;

internal static class ZoomDemoSettings
{
    public static void Apply(Control? viewer, ComboBox? button, ComboBox? position,
        NumericUpDown? offsetX, NumericUpDown? offsetY)
    {
        if (viewer is null || button is null || position is null || offsetX is null || offsetY is null) return;
        var panButton = button.SelectedIndex switch
        {
            1 => MouseButton.Right,
            2 => MouseButton.Middle,
            _ => MouseButton.Left
        };
        var (horizontal, vertical) = position.SelectedIndex switch
        {
            0 => (HorizontalAlignment.Left, VerticalAlignment.Top),
            1 => (HorizontalAlignment.Right, VerticalAlignment.Top),
            2 => (HorizontalAlignment.Left, VerticalAlignment.Bottom),
            4 => (HorizontalAlignment.Center, VerticalAlignment.Top),
            5 => (HorizontalAlignment.Center, VerticalAlignment.Bottom),
            _ => (HorizontalAlignment.Right, VerticalAlignment.Bottom)
        };
        var offset = new Vector((double)(offsetX.Value ?? 0), (double)(offsetY.Value ?? 0));
        if (viewer is ZoomView zoom)
        {
            zoom.SetCurrentValue(ZoomView.PanButtonProperty, panButton);
            zoom.SetCurrentValue(ZoomView.MinimapHorizontalAlignmentProperty, horizontal);
            zoom.SetCurrentValue(ZoomView.MinimapVerticalAlignmentProperty, vertical);
            zoom.SetCurrentValue(ZoomView.MinimapOffsetProperty, offset);
        }
        else if (viewer is OvoScrollView scroll)
        {
            scroll.SetCurrentValue(OvoScrollView.PanButtonProperty, panButton);
            scroll.SetCurrentValue(OvoScrollView.MinimapHorizontalAlignmentProperty, horizontal);
            scroll.SetCurrentValue(OvoScrollView.MinimapVerticalAlignmentProperty, vertical);
            scroll.SetCurrentValue(OvoScrollView.MinimapOffsetProperty, offset);
        }
    }
}

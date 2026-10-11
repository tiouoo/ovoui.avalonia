using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OvoUi.Common.Helpers;

namespace OvoUi.Demo.Views;

public partial class MacOsWindowControlsDemoWindow : Window
{
    public MacOsWindowControlsDemoWindow()
    {
        InitializeComponent();
        Opened += (_, _) => ApplyCurrentOptions();
    }

    private void UseSpacing(object? sender, RoutedEventArgs e)
    {
        var originX = Number(OriginX);
        var originY = Number(OriginY);
        var spacing = Number(Spacing);
        CloseX.Value = (decimal)originX;
        CloseY.Value = (decimal)originY;
        MinimizeX.Value = (decimal)(originX + spacing);
        MinimizeY.Value = (decimal)originY;
        ZoomX.Value = (decimal)(originX + spacing * 2);
        ZoomY.Value = (decimal)originY;
        ApplyCurrentOptions();
    }

    private void ApplyOptions(object? sender, RoutedEventArgs e) => ApplyCurrentOptions();

    private void ApplyCurrentOptions()
    {
        var options = new MacOsWindowControlsOptions
        {
            OriginX = Number(OriginX),
            OriginY = Number(OriginY),
            Spacing = Number(Spacing),
            Close = CreateButtonOptions(CloseX, CloseY, CloseWidth, CloseHeight, CloseOpacity, CloseVisible, CloseEnabled),
            Minimize = CreateButtonOptions(MinimizeX, MinimizeY, MinimizeWidth, MinimizeHeight, MinimizeOpacity, MinimizeVisible, MinimizeEnabled),
            Zoom = CreateButtonOptions(ZoomX, ZoomY, ZoomWidth, ZoomHeight, ZoomOpacity, ZoomVisible, ZoomEnabled)
        };

        StatusText.Text = MacOsWindowControls.TryApply(this, options, out var error)
            ? "Applied to the native window."
            : error;
    }

    private static MacOsWindowButtonOptions CreateButtonOptions(
        NumericUpDown x,
        NumericUpDown y,
        NumericUpDown width,
        NumericUpDown height,
        NumericUpDown opacity,
        CheckBox visible,
        CheckBox enabled) => new()
    {
        Position = new Point(Number(x), Number(y)),
        Size = new Size(Number(width), Number(height)),
        Opacity = Number(opacity),
        IsVisible = visible.IsChecked == true,
        IsEnabled = enabled.IsChecked == true
    };

    private static double Number(NumericUpDown input) => decimal.ToDouble(input.Value ?? 0);
}

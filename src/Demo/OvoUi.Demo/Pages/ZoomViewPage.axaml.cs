using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;

namespace OvoUi.Demo.Pages;

public partial class ZoomViewPage : UserControl
{
    public ZoomViewPage()
    {
        InitializeComponent();
        Options_OnChanged(this, new RoutedEventArgs());
        WheelMode_OnChanged(this, new RoutedEventArgs());
    }

    private void Options_OnChanged(object? sender, RoutedEventArgs e) =>
        ZoomDemoSettings.Apply(Viewer, PanButtonChoice, MinimapPositionChoice, MinimapOffsetX, MinimapOffsetY);

    private void WheelMode_OnChanged(object? sender, RoutedEventArgs e)
    {
        if (Viewer is not null && WheelZoomChoice is not null)
            Viewer.WheelZoomModifiers = WheelZoomChoice.SelectedIndex == 0 ? KeyModifiers.None : KeyModifiers.Control;
    }

    private void Zoom_OnClick(object? sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag as string == "in") Viewer.ZoomIn();
        else Viewer.ZoomOut();
    }

    private void Fit_OnClick(object? sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag as string == "fill") Viewer.FitToContent();
        else Viewer.FitToAll();
    }
}

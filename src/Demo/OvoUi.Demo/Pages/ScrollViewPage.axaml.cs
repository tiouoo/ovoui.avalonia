using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using OvoUi.SmoothScroll.Controls;

namespace OvoUi.Demo.Pages;

public partial class ScrollViewPage : UserControl
{
    public ScrollViewPage()
    {
        InitializeComponent();
        DataContext = new ScrollViewDemoModel();
    }

    private void Zoom_OnClick(object? sender, RoutedEventArgs e)
    {
        switch ((sender as Button)?.Tag as string)
        {
            case "in": ZoomView.ZoomBy(0.25); break;
            case "out": ZoomView.ZoomBy(-0.25); break;
            case "min": ZoomView.ZoomTo(ZoomView.MinZoomFactor); break;
            case "max": ZoomView.ZoomTo(ZoomView.MaxZoomFactor); break;
            default: ZoomView.ZoomTo(1); break;
        }
    }
}

public class ScrollViewDemoModel
{
    public static FuncValueConverter<bool?, ScrollMode> ScrollModeConverter { get; } =
        new(enabled => enabled == true ? ScrollMode.Enabled : ScrollMode.Disabled);

    public string ModeCode => """
        InitializeComponent();
        DataContext = new ScrollViewDemoModel();
        public static FuncValueConverter<bool?, ScrollMode> ScrollModeConverter { get; } =
            new(enabled => enabled == true ? ScrollMode.Enabled : ScrollMode.Disabled);
        """;

    public string ZoomCode => """
        // In the page constructor:
        InitializeComponent();
        DataContext = new ScrollViewDemoModel();

        private void Zoom_OnClick(object? sender, RoutedEventArgs e)
        {
            switch ((sender as Button)?.Tag as string)
            {
                case "in": ZoomView.ZoomBy(0.25); break;
                case "out": ZoomView.ZoomBy(-0.25); break;
                case "min": ZoomView.ZoomTo(ZoomView.MinZoomFactor); break;
                case "max": ZoomView.ZoomTo(ZoomView.MaxZoomFactor); break;
                default: ZoomView.ZoomTo(1); break;
            }
        }
        // Pass isAnimated: false to apply immediately.

        """;
}

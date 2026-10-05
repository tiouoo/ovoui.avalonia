using Avalonia.Controls;
using OvoUi.Demo.Models;

namespace OvoUi.Demo.Views;

public partial class DemoView : UserControl
{
    private const double CompactBreakpoint = 720;

    public DemoView()
    {
        InitializeComponent();
        DataContext = new DemoViewModel();
        SizeChanged += DemoView_OnSizeChanged;
    }

    private void DemoView_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var isCompact = e.NewSize.Width < CompactBreakpoint;
        NavMenu.IsHorizontalCollapsed = isCompact;
        NavMenu.EnableSearch = !isCompact;
    }
}

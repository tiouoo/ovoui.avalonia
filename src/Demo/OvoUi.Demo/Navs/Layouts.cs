using OvoUi.Demo.Models;
using OvoUi.Demo.Views.Pages;

namespace OvoUi.Demo.Navs;

public static class Layouts
{
    public static readonly List<Page> LayoutsList =
    [
        new() { Title = "AspectRatioLayout", Content = new BlankPage() },
        new() { Title = "CarouselPage", Content = new BlankPage() },
        new() { Title = "ColumnWrapPanel", Content = new BlankPage() },
        new() { Title = "ContentExpander", Content = new BlankPage() },
        new() { Title = "Divider", Content = new BlankPage() },
        new() { Title = "DrawerPage", Content = new BlankPage() },
        new() { Title = "ElasticWrapPanel", Content = new BlankPage() },
        new() { Title = "GlassCard", Content = new BlankPage() },
        new() { Title = "GridSplitter", Content = new BlankPage() },
        new() { Title = "GroupBorder", Content = new BlankPage() },
        new() { Title = "GroupBox", Content = new BlankPage() },
        new() { Title = "HeaderedContent", Content = new BlankPage() },
        new() { Title = "OvoGroupBox", Content = new BlankPage() },
        new() { Title = "ProportionalCanvas", Content = new BlankPage() },
        new() { Title = "Resizer", Content = new BlankPage() },
        new() { Title = "ScrollViewer", Content = new BlankPage() },
        new() { Title = "SmoothScroll", Content = new BlankPage() },
        new() { Title = "SplitView", Content = new BlankPage() },
        new() { Title = "ThemeVariantMapper", Content = new BlankPage() },
        new() { Title = "ThemeVariantScope", Content = new BlankPage() },
        new() { Title = "VirtualizingUniformGrid", Content = new BlankPage() },
        new() { Title = "WrapPanelWithTrailingItem", Content = new BlankPage() }
    ];
}

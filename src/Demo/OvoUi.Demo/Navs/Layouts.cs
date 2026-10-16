using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Layouts
{
    public static readonly List<Page> LayoutsList =
    [
        Page.View<StackPanelPage>("StackPanel"),
        Page.View<GridPage>("Grid"),
        Page.View<DockPanelPage>("DockPanel"),
        Page.View<WrapPanelPage>("WrapPanel"),
        Page.View<CanvasPage>("Canvas"),
        Page.View<UniformGridPage>("UniformGrid"),
        Page.View<VirtualizingStackPanelPage>("VirtualizingStackPanel"),
        Page.View<VirtualizingUniformGridPage>("VirtualizingUniformGrid"),
        Page.View<ColumnWrapPanelPage>("ColumnWrapPanel"),
        Page.View<ProportionalCanvasPage>("ProportionalCanvas"),
        Page.View<WrapPanelWithTrailingItemPage>("WrapPanelWithTrailingItem"),
        Page.View<GroupBorderPage>("GroupBorder"),
        Page.View<AspectRatioLayoutPage>("AspectRatioLayout"),
        Page.View<DividerPage>("Divider"),
        Page.View<ElasticWrapPanelPage>("ElasticWrapPanel"),
        Page.View<ElasticDecoratorPage>("ElasticDecorator"),
        Page.View<GridSplitterPage>("GridSplitter"),
        Page.View<ScrollViewerPage>("ScrollViewer"),
        Page.View<FadingScrollViewerPage>("FadingScrollViewer"),
        Page.View<ThemeVariantScopePage>("ThemeVariantScope"),
        Page.View<TabWindowPage>("OvoTabHost"),
        Page.View<HeaderedContentPage>("HeaderedContentControl"),
        Page.View<SplitViewPage>("SplitView"),
        Page.View<GroupBoxPage>("OvoGroupBox"),
    ];
}

using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Layouts
{
    public static readonly List<Page> LayoutsList =
    [
        Page.View<GroupBorderPage>("GroupBorder"),
        Page.View<AspectRatioLayoutPage>("AspectRatioLayout"),
        Page.View<DividerPage>("Divider"),
        Page.View<ElasticWrapPanelPage>("ElasticWrapPanel"),
        Page.View<GridSplitterPage>("GridSplitter"),
        Page.View<ScrollViewerPage>("ScrollViewer"),
        Page.View<ThemeVariantScopePage>("ThemeVariantScope"),
        Page.View<HeaderedContentPage>("HeaderedContentControl"),
        Page.View<SplitViewPage>("SplitView"),
        Page.View<GroupBoxPage>("OvoGroupBox"),
    ];
}

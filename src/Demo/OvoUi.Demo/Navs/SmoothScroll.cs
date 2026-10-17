using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class SmoothScroll
{
    public static readonly List<Page> Pages =
    [
        Page.View<SmoothScrollDemoPage>("Smooth Scroll Demo"),
        Page.View<SmoothScrollViewerPage>("Smooth ScrollViewer"),
        Page.View<ScrollViewPage>("OvoScrollView"),
        Page.View<InteractionTrackerPage>("InteractionTracker")
    ];
}

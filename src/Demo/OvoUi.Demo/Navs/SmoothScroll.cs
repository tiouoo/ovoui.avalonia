using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class SmoothScroll
{
    public static readonly List<Page> Pages =
    [
        Page.View<SmoothScrollDemoPage>("SmoothScroll"),
        Page.View<SmoothScrollViewerPage>("SmoothScrollViewer"),
        Page.View<ScrollViewPage>("OvoScrollView"),
        Page.View<InteractionTrackerPage>("InteractionTracker")
    ];
}
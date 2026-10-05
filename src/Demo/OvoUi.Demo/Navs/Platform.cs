using OvoUi.Demo.Models;
using OvoUi.Demo.Views.Pages;

namespace OvoUi.Demo.Navs;

public static class Platform
{
    public static readonly List<Page> PlatformList =
    [
        new() { Title = "MacOsWindowHandler", Content = new BlankPage() },
        new() { Title = "OvoTitleBar", Content = new BlankPage() },
        new() { Title = "OvoView", Content = new BlankPage() },
        new() { Title = "OvoWindow", Content = new BlankPage() },
        new() { Title = "PopupRenderContent", Content = new BlankPage() }
    ];
}

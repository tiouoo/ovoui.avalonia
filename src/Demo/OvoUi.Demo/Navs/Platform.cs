using OvoUi.Demo.Models;
using OvoUi.Demo.Views.Pages;

namespace OvoUi.Demo.Navs;

public static class Platform
{
    public static readonly List<Page> PlatformList =
    [
        new() { Title = "MacOsWindowHandler" },
        new() { Title = "OvoTitleBar" },
        new() { Title = "OvoView" },
        new() { Title = "OvoWindow" },
        new() { Title = "PopupRenderContent" }
    ];
}

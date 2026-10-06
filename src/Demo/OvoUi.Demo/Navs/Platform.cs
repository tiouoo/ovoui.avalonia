using OvoUi.Demo.Models;

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

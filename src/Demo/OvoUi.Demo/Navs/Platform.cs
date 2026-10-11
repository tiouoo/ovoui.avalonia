using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Platform
{
    public static readonly List<Page> PlatformList =
    [
        Page.View<MacOsWindowControlsPage>("MacOsWindowControls")
    ];
}

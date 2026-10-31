using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Platform
{
    public static readonly List<Page> PlatformsList =
    [
        Page.View<MacOsWindowControlsPage>("MacOsWindowControls"),
        Page.View<WindowsJumpListPage>("WindowsJumpList"),
        Page.View<WindowsSystemMenuPage>("WindowsSystemMenu"),
        Page.View<WindowContentInjectionPage>("WindowContentInjection"),
        Page.View<MemoryOptimizationPage>("MemoryOptimization")
    ];
}

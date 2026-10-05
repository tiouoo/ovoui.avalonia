using OvoUi.Demo.Models;
using OvoUi.Demo.Views.Pages;

namespace OvoUi.Demo.Navs;

public static class Menus
{
    public static readonly List<Page> MenusList =
    [
        new() { Title = "Anchor", Content = new BlankPage() },
        new() { Title = "Breadcrumb", Content = new BlankPage() },
        new() { Title = "CommandBar", Content = new BlankPage() },
        new() { Title = "ContextMenu", Content = new BlankPage() },
        new() { Title = "Menu", Content = new BlankPage() },
        new() { Title = "MenuFlyoutPresenter", Content = new BlankPage() },
        new() { Title = "MenuItem", Content = new BlankPage() },
        new() { Title = "NavMenu", Content = new BlankPage() },
        new() { Title = "Pagination", Content = new BlankPage() },
        new() { Title = "TabControl", Content = new BlankPage() },
        new() { Title = "TabbedPage", Content = new BlankPage() },
        new() { Title = "TabItem", Content = new BlankPage() },
        new() { Title = "TabStrip", Content = new BlankPage() },
        new() { Title = "ToolBar", Content = new BlankPage() }
    ];
}

using System.Collections.Generic;
using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Menus
{
    public static readonly List<Page> MenusList =
    [
        Page.View<MenuPage>("Menu"),
        Page.View<NavMenuPage>("NavMenu"),
        Page.View<BreadcrumbPage>("Breadcrumb"),
        Page.View<PaginationPage>("Pagination"),
        Page.View<AnchorPage>("Anchor"),
        Page.View<TabStripPage>("TabStrip"),
        Page.View<TabControlPage>("TabControl")
    ];
}
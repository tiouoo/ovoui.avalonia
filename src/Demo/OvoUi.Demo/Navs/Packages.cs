using System.Collections.Generic;
using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Packages
{
    public static readonly List<Page> Pages =
    [
        Page.Group("OvoUi.AvaloniaEdit", [
            Page.View<CodeBlockPage>("CodeBlock"),
            Page.View<TextEditorPage>("OvoTextEditor"),
        ]),
        Page.Group("OvoUi.AvaloniaEdit.Showcase", [
            Page.View<ControlShowcasePage>("ControlShowcase"),
        ]),
        Page.Group("OvoUi.Platform", Platform.PlatformsList),
        Page.Group("OvoUi.SmoothScroll", SmoothScroll.Pages),
        Page.Group("OvoUi.Tab", [Page.View<TabWindowPage>("OvoTabHost")]),
        Page.View<FontsPage>("OvoUi.Fonts"),
        Page.View<LangPage>("OvoUi.Lang"),
    ];
}

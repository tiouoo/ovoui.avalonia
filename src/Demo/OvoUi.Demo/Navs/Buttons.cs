using OvoUi.Demo.Models;
using OvoUi.Demo.Views.Pages;

namespace OvoUi.Demo.Navs;

public static class Buttons
{
    public static readonly List<Page> ButtonsList =
    [
        new() { Title = "Button", Content = new BlankPage() },
        new() { Title = "ButtonGroup", Content = new BlankPage() },
        new() { Title = "DropDownButton", Content = new BlankPage() },
        new() { Title = "HyperlinkButton", Content = new BlankPage() },
        new() { Title = "IconButton", Content = new BlankPage() },
        new() { Title = "IconDropDownButton", Content = new BlankPage() },
        new() { Title = "IconRepeatButton", Content = new BlankPage() },
        new() { Title = "IconSplitButton", Content = new BlankPage() },
        new() { Title = "IconToggleButton", Content = new BlankPage() },
        new() { Title = "RadioButton", Content = new BlankPage() },
        new() { Title = "RepeatButton", Content = new BlankPage() },
        new() { Title = "ScrollToButton", Content = new BlankPage() },
        new() { Title = "SplitButton", Content = new BlankPage() },
        new() { Title = "ToggleButton", Content = new BlankPage() },
        new() { Title = "ToggleSwitch", Content = new BlankPage() }
    ];
}

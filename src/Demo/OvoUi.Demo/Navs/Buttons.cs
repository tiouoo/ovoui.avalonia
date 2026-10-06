using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Buttons
{
    public static readonly List<Page> ButtonsList =
    [
        Page.View<ButtonPage>("Button"),
        Page.View<ButtonGroupPage>("ButtonGroup"),
        Page.View<CheckBoxPage>("CheckBox"),
        Page.View<DropDownButtonPage>("DropDownButton"),
        Page.View<HyperlinkButtonPage>("HyperlinkButton"),
        Page.View<IconButtonPage>("IconButton"),
        Page.View<RadioButtonPage>("RadioButton"),
        Page.View<ScrollToButtonPage>("ScrollToButton"),
        Page.View<SplitButtonPage>("SplitButton"),
        Page.View<ToggleButtonPage>("ToggleButton"),
        Page.View<ToggleSwitchPage>("ToggleSwitch")
    ];
}

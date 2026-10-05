using OvoUi.Demo.Models;
using OvoUi.Demo.Views.Pages;

namespace OvoUi.Demo.Navs;

public static class Basics
{
    public static readonly List<Page> BasicsList =
    [
        new() { Title = "AdornerLayer", Content = new BlankPage() },
        new() { Title = "Border", Content = new BlankPage() },
        new() { Title = "ContentPage", Content = new BlankPage() },
        new() { Title = "EmbeddableControlRoot", Content = new BlankPage() },
        new() { Title = "HeaderedContentControl", Content = new BlankPage() },
        new() { Title = "ItemsControl", Content = new BlankPage() },
        new() { Title = "Label", Content = new BlankPage() },
        new() { Title = "PathIcon", Content = new BlankPage() },
        new() { Title = "Popup", Content = new BlankPage() },
        new() { Title = "SelectableTextBlock", Content = new BlankPage() },
        new() { Title = "Separator", Content = new BlankPage() },
        new() { Title = "TextBlock", Content = new BlankPage() },
        new() { Title = "TransitioningContentControl", Content = new BlankPage() }
    ];
}

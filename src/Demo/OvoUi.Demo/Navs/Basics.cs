using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Basics
{
    public static readonly List<Page> BasicsList =
    [
        new() { Title = "Label", Content = new LabelPage() }
    ];
}
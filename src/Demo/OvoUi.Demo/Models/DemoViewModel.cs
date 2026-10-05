using Avalonia.Controls;
using OvoUi.Demo.Navs;
using OvoUi.Demo.Views.Pages;

namespace OvoUi.Demo.Models;

public sealed class DemoViewModel : ModelBase
{
    public DemoViewModel()
    {
        foreach (var page in Pages)
        {
            if (page.Children is { Count: > 0 })
            {
                Items += page.Children.Count;
                page.Children = page.Children.OrderBy(child => child.Title).ToList();
            }
            else
            {
                Items += 1;
            }
        }
        SelectedPage = Pages[0];
    }

    public static IReadOnlyList<Page> Pages { get; } =
    [
        new()
        {
            Title = "Overview",
            Content = new OverviewPage()
        },
        new() { Title = "Basic", Children = Basics.BasicsList },
        new() { Title = "Button", Children = Buttons.ButtonsList },
        new() { Title = "Input", Children = Inputs.InputsList },
        new() { Title = "Menu", Children = Menus.MenusList },
        new() { Title = "Time", Children = Times.TimesList },
        new() { Title = "Show", Children = Shows.ShowsList },
        new() { Title = "Feedback", Children = Feedbacks.FeedbacksList },
        new() { Title = "Layout", Children = Layouts.LayoutsList },
        new() { Title = "Platform", Children = Platform.PlatformList }
    ];

    public Page SelectedPage
    {
        get;
        set => SetField(ref field, value);
    }
    
    public int Items { get; set; }

}

public sealed class Page
{
    public required string Title { get; init; }
    
    public UserControl? Content { get; init; }

    public List<Page>? Children { get; set; }
}

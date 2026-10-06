using Avalonia.Controls;
using OvoUi.Demo.Navs;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Models;

public sealed class DemoViewModel : ModelBase
{
    private readonly List<Page> _navigationOrder;

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
        _navigationOrder = Pages
            .SelectMany(page => page.Children is { Count: > 0 } ? page.Children : [page])
            .ToList();
        SelectedPage = Pages[0];
    }

    public static IReadOnlyList<Page> Pages { get; } =
    [
        Page.View<OverviewPage>("Overview"),
        Page.Group("Basic", Basics.BasicsList),
        Page.Group("Button", Buttons.ButtonsList),
        Page.Group("Input", Inputs.InputsList),
        Page.Group("Menu", Menus.MenusList),
        Page.Group("Time", Times.TimesList),
        Page.Group("Show", Shows.ShowsList),
        Page.Group("Feedback", Feedbacks.FeedbacksList),
        Page.Group("Layout", Layouts.LayoutsList),
        Page.Group("Platform", Platform.PlatformList)
    ];

    public Page SelectedPage
    {
        get;
        set
        {
            if (ReferenceEquals(field, value))
                return;

            IsTransitionReversed = field is not null &&
                                   _navigationOrder.IndexOf(value) < _navigationOrder.IndexOf(field);
            SetField(ref field, value);
        }
    }

    public bool IsTransitionReversed
    {
        get;
        private set => SetField(ref field, value);
    }
    
    public int Items { get; set; }

}

public sealed class Page
{
    private readonly Lazy<UserControl>? _content;

    private Page(string title, Func<UserControl>? contentFactory = null)
    {
        Title = title;
        if (contentFactory is not null)
            _content = new Lazy<UserControl>(contentFactory);
    }

    public string Title { get; }

    // Lazy<T> creates the view once on first access and returns the same instance afterwards.
    public UserControl? Content => _content?.Value;

    public List<Page>? Children { get; set; }

    public static Page View<T>(string title) where T : UserControl, new() =>
        new(title, static () => new T());

    public static Page View(string title, Func<UserControl> contentFactory) =>
        new(title, contentFactory ?? throw new ArgumentNullException(nameof(contentFactory)));

    public static Page Group(string title, List<Page> children) =>
        new(title) { Children = children };
}

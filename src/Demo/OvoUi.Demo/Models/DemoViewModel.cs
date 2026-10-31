using System;
using System.Collections.Generic;
using Avalonia.Controls;
using OvoUi.Demo.Navs;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Models;

public sealed class DemoViewModel : ModelBase
{
    public DemoViewModel()
    {
        Items = CountItems(Pages);

        foreach (var page in Pages)
        {
            if (page.Children is { Count: > 0 })
            {
                SortChildren(page);
            }
        }

        SelectedPage = Pages[0];
        SearchText += $"  ({Items} items)";
    }

    private static int CountItems(IEnumerable<Page>? items)
    {
        if (items is null)
        {
            return 0;
        }

        var count = 0;

        foreach (var item in items)
        {
            if (item.Children is { Count: > 0 })
            {
                count += CountItems(item.Children);
            }
            else
            {
                count++;
            }
        }

        return count;
    }

    private static void SortChildren(Page page)
    {
        if (!page.AutoSortChildren || page.Children is not { Count: > 0 })
        {
            return;
        }

        page.Children = [.. page.Children
            .OrderBy(child => child.Children is not { Count: > 0 })
            .ThenBy(child => child.Title, StringComparer.OrdinalIgnoreCase)];

        foreach (var child in page.Children)
        {
            SortChildren(child);
        }
    }

    public string SearchText
    {
        get;
        set => SetField(ref field, value);
    } = "Search...";

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
        Page.Group("Packages", Packages.Pages, autoSortChildren: false)
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
    private readonly Lazy<UserControl>? _content;

    private Page(string title, Func<UserControl>? contentFactory = null)
    {
        Title = title;
        if (contentFactory is not null)
            _content = new Lazy<UserControl>(contentFactory);
    }

    public string Title { get; }

    public UserControl? Content => _content?.Value;

    public List<Page>? Children { get; set; }

    public bool AutoSortChildren { get; private init; } = true;

    public static Page View<T>(string title) where T : UserControl, new() =>
        new(title, static () => new T());

    public static Page View(string title, Func<UserControl> contentFactory) =>
        new(title, contentFactory ?? throw new ArgumentNullException(nameof(contentFactory)));

    public static Page Group(string title, List<Page> children, bool autoSortChildren = true) =>
        new(title) { Children = children, AutoSortChildren = autoSortChildren };
}

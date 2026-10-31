using System;
using System.Collections.Generic;
using Avalonia.Controls;
using OvoUi.Demo.Classic.Pages;

namespace OvoUi.Demo.Classic.Models;

public sealed class DemoViewModel : ModelBase
{
    public DemoViewModel()
    {
        SelectedPage = Pages[0];
        SearchText += $"  ({Items} items)";
    }

    public string SearchText
    {
        get;
        set => SetField(ref field, value);
    } = "Search...";

    public static IReadOnlyList<Page> Pages { get; } =
    [
        Page.View<DataGridPage>("DataGrid"),
        Page.View<WindowCompositionMaterialPage>("WindowCompositionMaterial"),
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

    public static Page View<T>(string title) where T : UserControl, new() =>
        new(title, static () => new T());

    public static Page View(string title, Func<UserControl> contentFactory) =>
        new(title, contentFactory ?? throw new ArgumentNullException(nameof(contentFactory)));

    public static Page Group(string title, List<Page> children) =>
        new(title) { Children = children };
}

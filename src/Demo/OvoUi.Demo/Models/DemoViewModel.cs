using Avalonia.Controls;
using OvoUi.Demo.Views.Pages;

namespace OvoUi.Demo.Models;

public sealed class DemoViewModel : ModelBase
{
    private Page _selectedPage;

    public DemoViewModel()
    {
        Pages =
        [
            new Page
            {
                Title = "Example",
                Icon = "M4,3 H20 A1,1 0 0 1 21,4 V20 A1,1 0 0 1 20,21 H4 A1,1 0 0 1 3,20 V4 A1,1 0 0 1 4,3 M7,7 H17 M7,11 H17 M7,15 H13",
                Content = new ExamplePage()
            }
        ];

        _selectedPage = Pages[0];
    }

    public IReadOnlyList<Page> Pages { get; }

    public Page SelectedPage
    {
        get => _selectedPage;
        set => SetField(ref _selectedPage, value);
    }
}

public sealed class Page
{
    public required string Title { get; init; }

    public string? Icon { get; init; }

    public required UserControl Content { get; init; }

    public IReadOnlyList<Page>? Children { get; init; }
}

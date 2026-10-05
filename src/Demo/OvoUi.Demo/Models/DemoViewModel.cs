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
    
    public required UserControl Content { get; init; }

    public IReadOnlyList<Page>? Children { get; init; }
}

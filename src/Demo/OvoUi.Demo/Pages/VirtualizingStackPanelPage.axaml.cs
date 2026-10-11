using Avalonia.Controls;

namespace OvoUi.Demo.Pages;

public partial class VirtualizingStackPanelPage : UserControl
{
    public VirtualizingStackPanelPage()
    {
        InitializeComponent();
        Items = Enumerable.Range(1, 100_000).Select(i => new VirtualItem(i, $"Log entry {i:N0}")).ToArray();
        DataContext = this;
    }

    public IReadOnlyList<VirtualItem> Items { get; }

    public string SourceCode { get; } =
        """public IReadOnlyList<VirtualItem> Items { get; } = Enumerable.Range(1, 100_000).Select(i => new VirtualItem(i, $"Log entry {i:N0}")).ToArray();""";
}

public sealed record VirtualItem(int Id, string Title);
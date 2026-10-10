using Avalonia.Controls;
namespace OvoUi.Demo.Pages;

public partial class DescriptionsPage : UserControl
{
    public DescriptionsPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public IReadOnlyList<DescriptionEntry> ProductDetails { get; } =
    [
        new("Product", "OvoUi Desktop"),
        new("Version", "2.4.0"),
        new("Release channel", "Stable"),
        new("Runtime", ".NET 10"),
        new("License", "MIT"),
        new("Last updated", "October 10, 2026"),
    ];

    public string SourceCode { get; } = """
        public IReadOnlyList<DescriptionEntry> ProductDetails { get; } =
        [
            new("Product", "OvoUi Desktop"),
            new("Version", "2.4.0"),
            new("Release channel", "Stable"),
            new("Runtime", ".NET 10"),
            new("License", "MIT"),
        ];

        public record DescriptionEntry(string Label, string Value);
        """;
}

public record DescriptionEntry(string Label, string Value);


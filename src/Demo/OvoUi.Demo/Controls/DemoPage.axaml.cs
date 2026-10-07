using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace OvoUi.Demo.Controls;

public partial class DemoPage : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<DemoPage, string?>(nameof(Text));

    public DemoPage()
    {
        InitializeComponent();
    }

    [Content]
    public AvaloniaList<Control> PageContent { get; } = [];

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace OvoUi.Demo.Classic.Controls;

public partial class DemoPage : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<DemoPage, string?>(nameof(Text));

    public static readonly StyledProperty<Thickness> TitleMarginProperty =
        AvaloniaProperty.Register<DemoPage, Thickness>(
            nameof(TitleMargin),
            new Thickness(0, 0, 0, 20));

    public DemoPage()
    {
        InitializeComponent();
    }

    [Content] public AvaloniaList<Control> PageContent { get; } = [];

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Thickness TitleMargin
    {
        get => GetValue(TitleMarginProperty);
        set => SetValue(TitleMarginProperty, value);
    }

}

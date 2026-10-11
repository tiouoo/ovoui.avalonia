using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using OvoUi.Demo.Views;

namespace OvoUi.Demo.Controls;

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

    [Content]
    public AvaloniaList<Control> PageContent { get; } = [];

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetCurrentValue(
            TitleMarginProperty,
            this.FindAncestorOfType<MainView>() is not null
                ? new Thickness(0, 6, 0, 20)
                : new Thickness(0, 0, 0, 20));
    }
}

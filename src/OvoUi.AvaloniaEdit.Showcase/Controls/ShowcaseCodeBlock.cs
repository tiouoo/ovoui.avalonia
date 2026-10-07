using Avalonia;

namespace OvoUi.AvaloniaEdit.Showcase.Controls;

/// <summary>
/// Describes a code block displayed by <see cref="ControlShowcase" />.
/// </summary>
public sealed class ShowcaseCodeBlock : AvaloniaObject
{
    public static readonly StyledProperty<string?> CodeProperty =
        AvaloniaProperty.Register<ShowcaseCodeBlock, string?>(nameof(Code));

    public static readonly StyledProperty<string> LanguageProperty =
        AvaloniaProperty.Register<ShowcaseCodeBlock, string>(nameof(Language), "text");

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<ShowcaseCodeBlock, object?>(nameof(Header));

    public static readonly StyledProperty<bool> ShowLineNumbersProperty =
        AvaloniaProperty.Register<ShowcaseCodeBlock, bool>(nameof(ShowLineNumbers), true);

    public static readonly StyledProperty<bool> WordWrapProperty =
        AvaloniaProperty.Register<ShowcaseCodeBlock, bool>(nameof(WordWrap));

    public static readonly StyledProperty<double> CodeHeightProperty =
        AvaloniaProperty.Register<ShowcaseCodeBlock, double>(nameof(CodeHeight), double.NaN);

    /// <summary>
    /// Gets or sets the source text. A null value uses the showcase's generated or explicit AXAML source.
    /// </summary>
    public string? Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public string Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    /// <summary>
    /// Gets or sets custom header content. When null, <see cref="Language" /> is displayed.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public bool ShowLineNumbers
    {
        get => GetValue(ShowLineNumbersProperty);
        set => SetValue(ShowLineNumbersProperty, value);
    }

    public bool WordWrap
    {
        get => GetValue(WordWrapProperty);
        set => SetValue(WordWrapProperty, value);
    }

    /// <summary>
    /// Gets or sets this block's preferred height. NaN uses <see cref="ControlShowcase.CodeHeight" />.
    /// </summary>
    public double CodeHeight
    {
        get => GetValue(CodeHeightProperty);
        set => SetValue(CodeHeightProperty, value);
    }
}

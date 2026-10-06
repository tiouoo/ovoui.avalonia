using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit.Highlighting;
using OvoUi.AvaloniaEdit.Highlighting;
using TextMateSharp.Grammars;
using TextMateInstallation = AvaloniaEdit.TextMate.TextMate.Installation;

namespace OvoUi.AvaloniaEdit.Controls;

[PseudoClasses(PC_Copied)]
[TemplatePart(PART_Editor, typeof(OvoTextEditor))]
[TemplatePart(PART_CopyButton, typeof(Button))]
public class CodeBlock : TemplatedControl
{
    public const string PART_Editor = "PART_Editor";
    public const string PART_CopyButton = "PART_CopyButton";
    public const string PC_Copied = ":copied";

    private static readonly Dictionary<string, string> LanguageAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["bash"] = "shellscript",
        ["shell"] = "shellscript",
        ["sh"] = "shellscript",
        ["zsh"] = "shellscript",
        ["c#"] = "csharp",
        ["cs"] = "csharp",
        ["js"] = "javascript",
        ["ts"] = "typescript",
        ["py"] = "python",
        ["rb"] = "ruby",
        ["yml"] = "yaml",
        ["xaml"] = "xml"
    };

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<CodeBlock, string>(
            nameof(Text), string.Empty, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> LanguageProperty =
        AvaloniaProperty.Register<CodeBlock, string>(nameof(Language), "text");

    public static readonly StyledProperty<string?> HighlightingLanguageProperty =
        AvaloniaProperty.Register<CodeBlock, string?>(nameof(HighlightingLanguage));

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<CodeBlock, object?>(nameof(Header));

    public static readonly StyledProperty<bool> IsHeaderSelectableProperty =
        AvaloniaProperty.Register<CodeBlock, bool>(nameof(IsHeaderSelectable));

    public static readonly StyledProperty<bool> ShowCopyButtonProperty =
        AvaloniaProperty.Register<CodeBlock, bool>(nameof(ShowCopyButton), true);

    public static readonly StyledProperty<string?> CopiedTextProperty =
        AvaloniaProperty.Register<CodeBlock, string?>(nameof(CopiedText));

    public static readonly StyledProperty<object?> CopyToolTipProperty =
        AvaloniaProperty.Register<CodeBlock, object?>(nameof(CopyToolTip));

    public static readonly StyledProperty<TimeSpan> CopyFeedbackDurationProperty =
        AvaloniaProperty.Register<CodeBlock, TimeSpan>(nameof(CopyFeedbackDuration), TimeSpan.FromSeconds(2));

    public static readonly StyledProperty<bool> UseTextMateProperty =
        AvaloniaProperty.Register<CodeBlock, bool>(nameof(UseTextMate), true);

    public static readonly StyledProperty<ThemeName> LightTextMateThemeProperty =
        AvaloniaProperty.Register<CodeBlock, ThemeName>(nameof(LightTextMateTheme), ThemeName.AtomOneLight);

    public static readonly StyledProperty<ThemeName> DarkTextMateThemeProperty =
        AvaloniaProperty.Register<CodeBlock, ThemeName>(nameof(DarkTextMateTheme), ThemeName.AtomOneDark);

    public static readonly DirectProperty<CodeBlock, OvoTextEditor?> EditorProperty =
        AvaloniaProperty.RegisterDirect<CodeBlock, OvoTextEditor?>(nameof(Editor), control => control.Editor);

    public static readonly DirectProperty<CodeBlock, object?> EffectiveHeaderProperty =
        AvaloniaProperty.RegisterDirect<CodeBlock, object?>(nameof(EffectiveHeader), control => control.EffectiveHeader);

    private OvoTextEditor? _editor;
    private Button? _copyButton;
    private object? _effectiveHeader = "text";
    private RegistryOptions? _registryOptions;
    private TextMateInstallation? _textMateInstallation;
    private CancellationTokenSource? _copyFeedbackCancellation;
    private bool _synchronizingText;
    private global::AvaloniaEdit.TextEditorOptions? _defaultEditorOptions;

    public CodeBlock()
    {
        ActualThemeVariantChanged += (_, _) =>
        {
            ApplyTextMateTheme();
            ApplyHighlighting();
        };
    }

    public event EventHandler? EditorReady;
    public event Action<Exception>? HighlightingFailed;
    public event Action<Exception>? CopyFailed;

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the language displayed in the header and used for highlighting by default.
    /// </summary>
    public string Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    /// <summary>
    /// Overrides the language used for highlighting without changing the displayed language.
    /// </summary>
    public string? HighlightingLanguage
    {
        get => GetValue(HighlightingLanguageProperty);
        set => SetValue(HighlightingLanguageProperty, value);
    }

    /// <summary>
    /// Gets or sets custom header content. When unset, <see cref="Language" /> is displayed.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public bool IsHeaderSelectable
    {
        get => GetValue(IsHeaderSelectableProperty);
        set => SetValue(IsHeaderSelectableProperty, value);
    }

    public bool ShowCopyButton
    {
        get => GetValue(ShowCopyButtonProperty);
        set => SetValue(ShowCopyButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets the copy-success label. The default comes from the OvoUi language resources.
    /// </summary>
    public string? CopiedText
    {
        get => GetValue(CopiedTextProperty);
        set => SetValue(CopiedTextProperty, value);
    }

    public object? CopyToolTip
    {
        get => GetValue(CopyToolTipProperty);
        set => SetValue(CopyToolTipProperty, value);
    }

    public TimeSpan CopyFeedbackDuration
    {
        get => GetValue(CopyFeedbackDurationProperty);
        set => SetValue(CopyFeedbackDurationProperty, value);
    }

    public bool UseTextMate
    {
        get => GetValue(UseTextMateProperty);
        set => SetValue(UseTextMateProperty, value);
    }

    public ThemeName LightTextMateTheme
    {
        get => GetValue(LightTextMateThemeProperty);
        set => SetValue(LightTextMateThemeProperty, value);
    }

    public ThemeName DarkTextMateTheme
    {
        get => GetValue(DarkTextMateThemeProperty);
        set => SetValue(DarkTextMateThemeProperty, value);
    }

    /// <summary>
    /// Gets the actual AvaloniaEdit instance after the control template has been applied.
    /// </summary>
    public OvoTextEditor? Editor => _editor;

    public object? EffectiveHeader => _effectiveHeader;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_editor is not null)
            _editor.TextChanged -= OnEditorTextChanged;
        if (_copyButton is not null)
            _copyButton.Click -= OnCopyButtonClick;

        DisposeTextMate();

        base.OnApplyTemplate(e);

        var oldEditor = _editor;
        _editor = e.NameScope.Find<OvoTextEditor>(PART_Editor);
        _copyButton = e.NameScope.Find<Button>(PART_CopyButton);

        RaisePropertyChanged(EditorProperty, oldEditor, _editor);

        if (_editor is null)
            return;

        _defaultEditorOptions = _editor.Options;
        _editor.Text = Text;
        _editor.TextChanged += OnEditorTextChanged;

        if (_copyButton is not null)
            _copyButton.Click += OnCopyButtonClick;

        ApplyEditorSettings();
        ApplyHighlighting();
        EditorReady?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_editor is null)
            return;

        ApplyHighlighting();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _copyFeedbackCancellation?.Cancel();
        _copyFeedbackCancellation?.Dispose();
        _copyFeedbackCancellation = null;
        PseudoClasses.Set(PC_Copied, false);
        DisposeTextMate();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty)
        {
            if (!_synchronizingText && _editor is not null && _editor.Text != Text)
                _editor.Text = Text;
        }
        else if (change.Property == HeaderProperty || change.Property == LanguageProperty)
        {
            UpdateEffectiveHeader();
            if (change.Property == LanguageProperty)
                ApplyHighlighting();
        }
        else if (change.Property == HighlightingLanguageProperty ||
                 change.Property == UseTextMateProperty ||
                 change.Property == AvaloniaEditor.SyntaxHighlightingProperty)
        {
            ApplyHighlighting();
        }
        else if (change.Property == LightTextMateThemeProperty ||
                 change.Property == DarkTextMateThemeProperty)
        {
            ApplyTextMateTheme();
        }
        else if (IsEditorSetting(change.Property))
        {
            ApplyEditorSettings();
        }
    }

    private static bool IsEditorSetting(AvaloniaProperty property)
    {
        return property == AvaloniaEditor.ShowLineNumbersProperty ||
               property == AvaloniaEditor.WordWrapProperty ||
               property == AvaloniaEditor.IsReadOnlyProperty ||
               property == AvaloniaEditor.RightClickMovesCaretProperty ||
               property == AvaloniaEditor.FontFamilyProperty ||
               property == AvaloniaEditor.FontSizeProperty ||
               property == AvaloniaEditor.FontWeightProperty ||
               property == AvaloniaEditor.HorizontalScrollBarVisibilityProperty ||
               property == AvaloniaEditor.VerticalScrollBarVisibilityProperty ||
               property == AvaloniaEditor.OptionsProperty ||
               property == AvaloniaEditor.SelectionBrushProperty ||
               property == AvaloniaEditor.SelectionForegroundProperty ||
               property == AvaloniaEditor.CaretBrushProperty ||
               property == AvaloniaEditor.LineNumbersForegroundProperty;
    }

    private void UpdateEffectiveHeader()
    {
        var oldValue = _effectiveHeader;
        _effectiveHeader = Header ?? Language;
        if (!Equals(oldValue, _effectiveHeader))
            RaisePropertyChanged(EffectiveHeaderProperty, oldValue, _effectiveHeader);
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        if (_editor is null || Text == _editor.Text)
            return;

        _synchronizingText = true;
        SetCurrentValue(TextProperty, _editor.Text);
        _synchronizingText = false;
    }

    private void ApplyEditorSettings()
    {
        if (_editor is null)
            return;

        _editor.ShowLineNumbers = AvaloniaEditor.GetShowLineNumbers(this);
        _editor.WordWrap = AvaloniaEditor.GetWordWrap(this);
        _editor.IsReadOnly = AvaloniaEditor.GetIsReadOnly(this);
        if (AvaloniaEditor.GetFontFamily(this) is { } fontFamily)
            _editor.FontFamily = fontFamily;
        else
            _editor.ClearValue(global::AvaloniaEdit.TextEditor.FontFamilyProperty);

        if (AvaloniaEditor.GetFontSize(this) is { } fontSize)
            _editor.FontSize = fontSize;
        else
            _editor.ClearValue(global::AvaloniaEdit.TextEditor.FontSizeProperty);

        if (AvaloniaEditor.GetFontWeight(this) is { } fontWeight)
            _editor.FontWeight = fontWeight;
        else
            _editor.ClearValue(global::AvaloniaEdit.TextEditor.FontWeightProperty);

        _editor.HorizontalScrollBarVisibility = AvaloniaEditor.GetHorizontalScrollBarVisibility(this);
        _editor.VerticalScrollBarVisibility = AvaloniaEditor.GetVerticalScrollBarVisibility(this);
        _editor.TextArea.RightClickMovesCaret = AvaloniaEditor.GetRightClickMovesCaret(this);

        _editor.Options = AvaloniaEditor.GetOptions(this) ?? _defaultEditorOptions ?? _editor.Options;

        ApplyOptionalBrush(
            AvaloniaEditor.GetSelectionBrush(this),
            value => _editor.TextArea.SelectionBrush = value,
            () => _editor.TextArea.ClearValue(global::AvaloniaEdit.Editing.TextArea.SelectionBrushProperty));

        ApplyOptionalBrush(
            AvaloniaEditor.GetSelectionForeground(this),
            value => _editor.TextArea.SelectionForeground = value,
            () => _editor.TextArea.ClearValue(global::AvaloniaEdit.Editing.TextArea.SelectionForegroundProperty));

        ApplyOptionalBrush(
            AvaloniaEditor.GetCaretBrush(this),
            value => _editor.TextArea.CaretBrush = value,
            () => _editor.TextArea.ClearValue(global::AvaloniaEdit.Editing.TextArea.CaretBrushProperty));

        ApplyOptionalBrush(
            AvaloniaEditor.GetLineNumbersForeground(this),
            value => _editor.LineNumbersForeground = value,
            () => _editor.ClearValue(global::AvaloniaEdit.TextEditor.LineNumbersForegroundProperty));
    }

    private static void ApplyOptionalBrush(IBrush? brush, Action<IBrush> apply, Action clear)
    {
        if (brush is null)
            clear();
        else
            apply(brush);
    }

    private void EnsureTextMate()
    {
        if (_editor is null || _textMateInstallation is not null)
            return;

        _registryOptions = new RegistryOptions(DarkTextMateTheme);
        _textMateInstallation = global::AvaloniaEdit.TextMate.TextMate.InstallTextMate(
            _editor,
            _registryOptions,
            exceptionHandler: exception => HighlightingFailed?.Invoke(exception));
        ApplyTextMateTheme();
    }

    private void ApplyTextMateTheme()
    {
        if (_textMateInstallation is null || _registryOptions is null)
            return;

        var theme = ActualThemeVariant == ThemeVariant.Dark ? DarkTextMateTheme : LightTextMateTheme;
        _textMateInstallation.SetTheme(_registryOptions.LoadTheme(theme));
    }

    private void ApplyHighlighting()
    {
        if (_editor is null)
            return;

        var explicitDefinition = AvaloniaEditor.GetSyntaxHighlighting(this);
        var language = string.IsNullOrWhiteSpace(HighlightingLanguage) ? Language : HighlightingLanguage;
        language ??= string.Empty;

        if (explicitDefinition is not null)
        {
            DisposeTextMate();
            _editor.SyntaxHighlighting = explicitDefinition;
            return;
        }

        if (OvoHighlightingProvider.Find(language, ActualThemeVariant == ThemeVariant.Dark) is { } ovoDefinition)
        {
            DisposeTextMate();
            _editor.SyntaxHighlighting = ovoDefinition;
            return;
        }

        if (UseTextMate)
        {
            EnsureTextMate();
            var scope = FindTextMateScope(language);
            if (!string.IsNullOrEmpty(scope) && _textMateInstallation is not null)
            {
                _editor.SyntaxHighlighting = null;
                _textMateInstallation.SetGrammar(scope);
                return;
            }
        }

        DisposeTextMate();
        _editor.SyntaxHighlighting = FindBuiltInDefinition(language);
    }

    private string? FindTextMateScope(string language)
    {
        if (_registryOptions is null || string.IsNullOrWhiteSpace(language))
            return null;

        var value = language.Trim();
        if (LanguageAliases.TryGetValue(value, out var alias))
            value = alias;

        if (value.StartsWith('.'))
            return _registryOptions.GetScopeByExtension(value);

        var match = _registryOptions.GetAvailableLanguages().FirstOrDefault(candidate =>
            candidate.Id.Equals(value, StringComparison.OrdinalIgnoreCase) ||
            candidate.Aliases?.Any(item => item.Equals(value, StringComparison.OrdinalIgnoreCase)) == true);

        match ??= _registryOptions.GetLanguageByExtension($".{value}");
        return match is null ? null : _registryOptions.GetScopeByLanguageId(match.Id);
    }

    private static IHighlightingDefinition? FindBuiltInDefinition(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return null;

        var value = language.Trim();
        return value.StartsWith('.')
            ? HighlightingManager.Instance.GetDefinitionByExtension(value)
            : HighlightingManager.Instance.GetDefinition(value);
    }

    private void DisposeTextMate()
    {
        _textMateInstallation?.Dispose();
        _textMateInstallation = null;
        _registryOptions = null;
    }

    private async void OnCopyButtonClick(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
            return;

        try
        {
            await clipboard.SetTextAsync(Text ?? string.Empty);
        }
        catch (Exception exception)
        {
            CopyFailed?.Invoke(exception);
            return;
        }

        _copyFeedbackCancellation?.Cancel();
        _copyFeedbackCancellation?.Dispose();
        var cancellation = new CancellationTokenSource();
        _copyFeedbackCancellation = cancellation;
        var token = cancellation.Token;

        PseudoClasses.Set(PC_Copied, true);

        try
        {
            var duration = CopyFeedbackDuration < TimeSpan.Zero ? TimeSpan.Zero : CopyFeedbackDuration;
            await Task.Delay(duration, token);
            if (!token.IsCancellationRequested)
                PseudoClasses.Set(PC_Copied, false);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(_copyFeedbackCancellation, cancellation))
                _copyFeedbackCancellation = null;
            cancellation.Dispose();
        }
    }
}

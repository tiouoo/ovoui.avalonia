using System.Xml.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using OvoUi.AvaloniaEdit.Controls;

namespace OvoUi.AvaloniaEdit.Showcase.Controls;

[TemplatePart(PART_TabStrip, typeof(TabStrip))]
[TemplatePart(PART_TransitionHost, typeof(TransitioningContentControl))]
public class ControlShowcase : ContentControl
{
    public const string PART_TabStrip = "PART_TabStrip";
    public const string PART_TransitionHost = "PART_TransitionHost";

    public static readonly StyledProperty<string?> SourceKeyProperty =
        AvaloniaProperty.Register<ControlShowcase, string?>(nameof(SourceKey));

    public static readonly StyledProperty<string?> CodeProperty =
        AvaloniaProperty.Register<ControlShowcase, string?>(nameof(Code));

    public static readonly StyledProperty<bool> AutoFormatCodeProperty =
        AvaloniaProperty.Register<ControlShowcase, bool>(nameof(AutoFormatCode), true);

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<ControlShowcase, int>(
            nameof(SelectedIndex), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> PreviewHeaderProperty =
        AvaloniaProperty.Register<ControlShowcase, object?>(nameof(PreviewHeader), "预览");

    public static readonly StyledProperty<object?> CodeHeaderProperty =
        AvaloniaProperty.Register<ControlShowcase, object?>(nameof(CodeHeader), "代码");

    public static readonly StyledProperty<object?> CodeBlockHeaderProperty =
        AvaloniaProperty.Register<ControlShowcase, object?>(nameof(CodeBlockHeader), "axaml");

    public static readonly StyledProperty<string> LanguageProperty =
        AvaloniaProperty.Register<ControlShowcase, string>(nameof(Language), "axaml");

    public static readonly StyledProperty<bool> ShowLineNumbersProperty =
        AvaloniaProperty.Register<ControlShowcase, bool>(nameof(ShowLineNumbers), true);

    public static readonly StyledProperty<bool> WordWrapProperty =
        AvaloniaProperty.Register<ControlShowcase, bool>(nameof(WordWrap), true);

    public static readonly StyledProperty<double> CodeHeightProperty =
        AvaloniaProperty.Register<ControlShowcase, double>(nameof(CodeHeight), 280d);

    public static readonly StyledProperty<double> PreviewMinHeightProperty =
        AvaloniaProperty.Register<ControlShowcase, double>(nameof(PreviewMinHeight), 160d);

    public static readonly StyledProperty<Thickness> PreviewPaddingProperty =
        AvaloniaProperty.Register<ControlShowcase, Thickness>(nameof(PreviewPadding), new Thickness(24));

    public static readonly StyledProperty<IPageTransition?> PageTransitionProperty =
        AvaloniaProperty.Register<ControlShowcase, IPageTransition?>(
            nameof(PageTransition), new SlideFadePageTransition());

    public static readonly DirectProperty<ControlShowcase, string> EffectiveCodeProperty =
        AvaloniaProperty.RegisterDirect<ControlShowcase, string>(
            nameof(EffectiveCode), control => control.EffectiveCode);

    public static readonly DirectProperty<ControlShowcase, CodeBlock?> CodeBlockProperty =
        AvaloniaProperty.RegisterDirect<ControlShowcase, CodeBlock?>(
            nameof(CodeBlock), control => control.CodeBlock);

    private TabStrip? _tabStrip;
    private TransitioningContentControl? _transitionHost;
    private ContentPresenter? _previewPresenter;
    private CodeBlock? _codeBlock;
    private string _effectiveCode = string.Empty;
    private int _displayedIndex;
    private bool _synchronizingSelection;

    public string? SourceKey
    {
        get => GetValue(SourceKeyProperty);
        set => SetValue(SourceKeyProperty, value);
    }

    /// <summary>
    /// Gets or sets explicit code. When null, generated source identified by <see cref="SourceKey" /> is used.
    /// </summary>
    public string? Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public bool AutoFormatCode
    {
        get => GetValue(AutoFormatCodeProperty);
        set => SetValue(AutoFormatCodeProperty, value);
    }

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public object? PreviewHeader
    {
        get => GetValue(PreviewHeaderProperty);
        set => SetValue(PreviewHeaderProperty, value);
    }

    public object? CodeHeader
    {
        get => GetValue(CodeHeaderProperty);
        set => SetValue(CodeHeaderProperty, value);
    }

    public object? CodeBlockHeader
    {
        get => GetValue(CodeBlockHeaderProperty);
        set => SetValue(CodeBlockHeaderProperty, value);
    }

    public string Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
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

    public double CodeHeight
    {
        get => GetValue(CodeHeightProperty);
        set => SetValue(CodeHeightProperty, value);
    }

    public double PreviewMinHeight
    {
        get => GetValue(PreviewMinHeightProperty);
        set => SetValue(PreviewMinHeightProperty, value);
    }

    public Thickness PreviewPadding
    {
        get => GetValue(PreviewPaddingProperty);
        set => SetValue(PreviewPaddingProperty, value);
    }

    public IPageTransition? PageTransition
    {
        get => GetValue(PageTransitionProperty);
        set => SetValue(PageTransitionProperty, value);
    }

    public string EffectiveCode => _effectiveCode;

    /// <summary>
    /// Gets the code block created by the template after it has been applied.
    /// </summary>
    public CodeBlock? CodeBlock => _codeBlock;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_tabStrip is not null)
            _tabStrip.SelectionChanged -= OnTabSelectionChanged;

        base.OnApplyTemplate(e);

        var oldCodeBlock = _codeBlock;
        _tabStrip = e.NameScope.Find<TabStrip>(PART_TabStrip);
        _transitionHost = e.NameScope.Find<TransitioningContentControl>(PART_TransitionHost);

        _previewPresenter = new ContentPresenter();
        _codeBlock = new CodeBlock
        {
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(0),
            HeaderCornerRadius = new CornerRadius(0),
            ShowCopyButton = true
        };
        RaisePropertyChanged(CodeBlockProperty, oldCodeBlock, _codeBlock);

        UpdatePreviewPresenter();
        UpdateEffectiveCode();
        UpdateCodeBlock();

        _displayedIndex = NormalizeIndex(SelectedIndex);
        if (_tabStrip is not null)
        {
            _tabStrip.SelectedIndex = _displayedIndex;
            _tabStrip.SelectionChanged += OnTabSelectionChanged;
        }

        if (_transitionHost is not null)
        {
            _transitionHost.PageTransition = PageTransition;
            _transitionHost.Content = GetPage(_displayedIndex);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SourceKeyProperty ||
            change.Property == CodeProperty ||
            change.Property == AutoFormatCodeProperty)
        {
            UpdateEffectiveCode();
            UpdateCodeBlock();
        }
        else if (change.Property == SelectedIndexProperty)
        {
            var normalized = NormalizeIndex(SelectedIndex);
            if (normalized != SelectedIndex)
            {
                SetCurrentValue(SelectedIndexProperty, normalized);
                return;
            }

            UpdateSelection(normalized);
        }
        else if (change.Property == ContentProperty ||
                 change.Property == ContentTemplateProperty ||
                 change.Property == HorizontalContentAlignmentProperty ||
                 change.Property == VerticalContentAlignmentProperty ||
                 change.Property == PreviewPaddingProperty ||
                 change.Property == PreviewMinHeightProperty)
        {
            UpdatePreviewPresenter();
        }
        else if (change.Property == LanguageProperty ||
                 change.Property == CodeBlockHeaderProperty ||
                 change.Property == ShowLineNumbersProperty ||
                 change.Property == WordWrapProperty ||
                 change.Property == CodeHeightProperty)
        {
            UpdateCodeBlock();
        }
        else if (change.Property == PageTransitionProperty && _transitionHost is not null)
        {
            _transitionHost.PageTransition = PageTransition;
        }
    }

    private void OnTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_synchronizingSelection || _tabStrip is null)
            return;

        SetCurrentValue(SelectedIndexProperty, NormalizeIndex(_tabStrip.SelectedIndex));
    }

    private void UpdateSelection(int selectedIndex)
    {
        if (_tabStrip is not null && _tabStrip.SelectedIndex != selectedIndex)
        {
            _synchronizingSelection = true;
            _tabStrip.SelectedIndex = selectedIndex;
            _synchronizingSelection = false;
        }

        if (_transitionHost is null || selectedIndex == _displayedIndex)
            return;

        _transitionHost.IsTransitionReversed = selectedIndex < _displayedIndex;
        _displayedIndex = selectedIndex;
        _transitionHost.Content = GetPage(selectedIndex);
    }

    private object? GetPage(int selectedIndex) => selectedIndex == 0 ? _previewPresenter : _codeBlock;

    private void UpdatePreviewPresenter()
    {
        if (_previewPresenter is null)
            return;

        _previewPresenter.Content = Content;
        _previewPresenter.ContentTemplate = ContentTemplate;
        _previewPresenter.HorizontalContentAlignment = HorizontalContentAlignment;
        _previewPresenter.VerticalContentAlignment = VerticalContentAlignment;
        _previewPresenter.Padding = PreviewPadding;
        _previewPresenter.MinHeight = PreviewMinHeight;
    }

    private void UpdateEffectiveCode()
    {
        var source = Code;
        if (source is null)
            ShowcaseSourceRegistry.TryGet(SourceKey, out source);

        source ??= string.Empty;
        if (AutoFormatCode)
            source = FormatXaml(source);

        SetAndRaise(EffectiveCodeProperty, ref _effectiveCode, source);
    }

    private void UpdateCodeBlock()
    {
        if (_codeBlock is null)
            return;

        _codeBlock.Text = EffectiveCode;
        _codeBlock.Language = Language;
        _codeBlock.Header = CodeBlockHeader;
        _codeBlock.Height = Math.Max(78d, CodeHeight);
        AvaloniaEditor.SetShowLineNumbers(_codeBlock, ShowLineNumbers);
        AvaloniaEditor.SetWordWrap(_codeBlock, WordWrap);
        AvaloniaEditor.SetHorizontalScrollBarVisibility(
            _codeBlock,
            WordWrap ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto);
        AvaloniaEditor.SetVerticalScrollBarVisibility(_codeBlock, ScrollBarVisibility.Auto);
        AvaloniaEditor.SetIsReadOnly(_codeBlock, true);
    }

    private static int NormalizeIndex(int value) => value <= 0 ? 0 : 1;

    private static string FormatXaml(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return string.Empty;

        var normalized = source.Replace("\r\n", "\n").Trim();
        try
        {
            return XDocument.Parse(normalized).ToString();
        }
        catch
        {
            return NormalizeIndentation(normalized);
        }
    }

    private static string NormalizeIndentation(string source)
    {
        var lines = source.Split('\n');
        var indentation = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.TakeWhile(char.IsWhiteSpace).Count())
            .DefaultIfEmpty(0)
            .Min();

        return string.Join(
            Environment.NewLine,
            lines.Select(line => line.Length >= indentation ? line[indentation..].TrimEnd() : string.Empty));
    }
}

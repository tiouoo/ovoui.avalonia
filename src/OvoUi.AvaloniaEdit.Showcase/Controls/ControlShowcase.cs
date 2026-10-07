using System.Xml.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using OvoUi.AvaloniaEdit.Controls;
using OvoUi.Theme.Animations;

namespace OvoUi.AvaloniaEdit.Showcase.Controls;

[PseudoClasses(PC_HeaderRight)]
[TemplatePart(PART_TabStrip, typeof(TabStrip))]
[TemplatePart(PART_TransitionHost, typeof(TransitioningContentControl))]
public class ControlShowcase : ContentControl
{
    public const string PC_HeaderRight = ":header-right";
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

    public static readonly StyledProperty<object?> CombinedHeaderProperty =
        AvaloniaProperty.Register<ControlShowcase, object?>(nameof(CombinedHeader), "分屏");

    public static readonly StyledProperty<object?> CodeBlockHeaderProperty =
        AvaloniaProperty.Register<ControlShowcase, object?>(nameof(CodeBlockHeader), "axaml");

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<ControlShowcase, object?>(nameof(Header));

    public static readonly StyledProperty<ShowcaseHeaderPlacement> HeaderPlacementProperty =
        AvaloniaProperty.Register<ControlShowcase, ShowcaseHeaderPlacement>(
            nameof(HeaderPlacement), ShowcaseHeaderPlacement.Left);

    public static readonly StyledProperty<ShowcaseHeightBehavior> HeightBehaviorProperty =
        AvaloniaProperty.Register<ControlShowcase, ShowcaseHeightBehavior>(
            nameof(HeightBehavior), ShowcaseHeightBehavior.Stable);

    public static readonly StyledProperty<string> LanguageProperty =
        AvaloniaProperty.Register<ControlShowcase, string>(nameof(Language), "axaml");

    public static readonly StyledProperty<bool> ShowLineNumbersProperty =
        AvaloniaProperty.Register<ControlShowcase, bool>(nameof(ShowLineNumbers), true);

    public static readonly StyledProperty<bool> WordWrapProperty =
        AvaloniaProperty.Register<ControlShowcase, bool>(nameof(WordWrap));

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
    private Grid? _combinedPage;
    private string _effectiveCode = string.Empty;
    private int _displayedIndex;
    private bool _synchronizingSelection;
    private double _previewHeight;
    private CancellationTokenSource? _heightAnimationCancellation;

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

    public object? CombinedHeader
    {
        get => GetValue(CombinedHeaderProperty);
        set => SetValue(CombinedHeaderProperty, value);
    }

    public object? CodeBlockHeader
    {
        get => GetValue(CodeBlockHeaderProperty);
        set => SetValue(CodeBlockHeaderProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public ShowcaseHeaderPlacement HeaderPlacement
    {
        get => GetValue(HeaderPlacementProperty);
        set => SetValue(HeaderPlacementProperty, value);
    }

    public ShowcaseHeightBehavior HeightBehavior
    {
        get => GetValue(HeightBehaviorProperty);
        set => SetValue(HeightBehaviorProperty, value);
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
        CancelHeightAnimation();
        if (_tabStrip is not null)
            _tabStrip.SelectionChanged -= OnTabSelectionChanged;
        if (_previewPresenter is not null)
            _previewPresenter.SizeChanged -= OnPreviewSizeChanged;

        base.OnApplyTemplate(e);
        UpdateHeaderPlacement();

        var oldCodeBlock = _codeBlock;
        _tabStrip = e.NameScope.Find<TabStrip>(PART_TabStrip);
        _transitionHost = e.NameScope.Find<TransitioningContentControl>(PART_TransitionHost);
        _combinedPage = null;

        _previewPresenter = new ContentPresenter();
        _previewPresenter.SizeChanged += OnPreviewSizeChanged;
        _codeBlock = new CodeBlock
        {
            CornerRadius = new CornerRadius(0),
            HeaderCornerRadius = new CornerRadius(0),
            ShowCopyButton = true
        };
        _codeBlock.Classes.Add("control-showcase-code-block");
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
        else if (change.Property == HeightBehaviorProperty)
        {
            CancelHeightAnimation();
            _transitionHost?.ClearValue(Layoutable.HeightProperty);
            if (_transitionHost is not null)
                _transitionHost.MinHeight = HeightBehavior == ShowcaseHeightBehavior.Animated
                    ? Math.Max(PreviewMinHeight, _previewHeight)
                    : 0d;
            UpdateCodeBlock();
        }
        else if (change.Property == HeaderPlacementProperty)
        {
            UpdateHeaderPlacement();
        }
    }

    private void OnTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_synchronizingSelection || _tabStrip is null)
            return;

        SetCurrentValue(SelectedIndexProperty, NormalizeIndex(_tabStrip.SelectedIndex));
    }

    private void UpdateHeaderPlacement()
    {
        PseudoClasses.Set(PC_HeaderRight, HeaderPlacement == ShowcaseHeaderPlacement.Right);
    }

    private void OnPreviewSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // The transition host stretches the preview through intermediate animation sizes;
        // only cache its natural height while no height animation is running.
        if (_displayedIndex == 0 &&
            _heightAnimationCancellation is null &&
            e.NewSize.Height > 0)
        {
            _previewHeight = e.NewSize.Height;
            if (_transitionHost is not null && HeightBehavior == ShowcaseHeightBehavior.Animated)
                _transitionHost.MinHeight = PreviewMinHeight;
        }
        SyncCodeBlockSize(e.NewSize);
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

        var startHeight = _transitionHost.Bounds.Height;
        var previousIndex = _displayedIndex;
        _transitionHost.IsTransitionReversed = selectedIndex < previousIndex;
        _displayedIndex = selectedIndex;
        TransitionToPage(previousIndex, selectedIndex);

        if (HeightBehavior == ShowcaseHeightBehavior.Animated)
            _ = AnimateContentHeightAsync(selectedIndex, startHeight);
    }

    private object? GetPage(int selectedIndex) => selectedIndex switch
    {
        0 => _previewPresenter,
        1 => _codeBlock,
        _ => CreateCombinedPage()
    };

    private Grid CreateCombinedPage()
    {
        var divider = new Border();
        divider.Classes.Add("control-showcase-divider");

        var page = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,1,*"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(divider, 1);
        Grid.SetColumn(_codeBlock!, 2);
        page.Children.Add(_previewPresenter!);
        page.Children.Add(divider);
        page.Children.Add(_codeBlock!);
        return _combinedPage = page;
    }

    private void TransitionToPage(int previousIndex, int selectedIndex)
    {
        if (_transitionHost is null)
            return;

        if (previousIndex != 2 && selectedIndex != 2)
        {
            _transitionHost.Content = GetPage(selectedIndex);
            return;
        }

        var snapshot = CaptureCurrentPage();
        if (snapshot is not null)
        {
            // The preview is a real control and cannot belong to both the old and combined
            // pages. Keep a bitmap of the old page in the transition while it is reparented.
            _transitionHost.PageTransition = null;
            _transitionHost.Content = snapshot;
        }
        else
        {
            _transitionHost.Content = null;
        }

        ResetTransitionState(_previewPresenter);
        ResetTransitionState(_codeBlock);
        _combinedPage?.Children.Clear();
        _combinedPage = null;
        _transitionHost.PageTransition = PageTransition;
        _transitionHost.Content = GetPage(selectedIndex);
    }

    private Image? CaptureCurrentPage()
    {
        if (_transitionHost is null || _transitionHost.Bounds.Width <= 0 || _transitionHost.Bounds.Height <= 0)
            return null;

        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d;
        var pixelSize = PixelSize.FromSize(_transitionHost.Bounds.Size, scaling);
        if (pixelSize.Width <= 0 || pixelSize.Height <= 0)
            return null;

        var bitmap = new RenderTargetBitmap(pixelSize, new Vector(96d * scaling, 96d * scaling));
        bitmap.Render(_transitionHost);
        return new Image
        {
            Source = bitmap,
            Stretch = Stretch.Fill,
            Width = _transitionHost.Bounds.Width,
            Height = _transitionHost.Bounds.Height
        };
    }

    private static void ResetTransitionState(Visual? visual)
    {
        if (visual is null)
            return;

        visual.RenderTransform = null;
        visual.Opacity = 1d;
        visual.IsVisible = true;
    }

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
        if (_previewPresenter is { Bounds.Width: > 0, Bounds.Height: > 0 })
            SyncCodeBlockSize(_previewPresenter.Bounds.Size);
        else if (HeightBehavior == ShowcaseHeightBehavior.Stable)
            _codeBlock.Height = Math.Max(78d, CodeHeight);
        AvaloniaEditor.SetShowLineNumbers(_codeBlock, ShowLineNumbers);
        AvaloniaEditor.SetWordWrap(_codeBlock, WordWrap);
        AvaloniaEditor.SetHorizontalScrollBarVisibility(
            _codeBlock,
            WordWrap ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto);
        AvaloniaEditor.SetVerticalScrollBarVisibility(_codeBlock, ScrollBarVisibility.Auto);
        AvaloniaEditor.SetIsReadOnly(_codeBlock, true);
    }

    private void SyncCodeBlockSize(Size previewSize)
    {
        if (_codeBlock is null || previewSize.Width <= 0 || previewSize.Height <= 0)
            return;

        _codeBlock.ClearValue(Layoutable.WidthProperty);
        if (HeightBehavior == ShowcaseHeightBehavior.Stable)
            _codeBlock.Height = previewSize.Height;
        else
            _codeBlock.ClearValue(Layoutable.HeightProperty);
    }

    private async Task AnimateContentHeightAsync(int selectedIndex, double startHeight)
    {
        if (_transitionHost is null || _previewPresenter is null)
            return;

        _heightAnimationCancellation?.Cancel();
        _heightAnimationCancellation?.Dispose();
        var cancellation = new CancellationTokenSource();
        _heightAnimationCancellation = cancellation;

        // Let the preview measure without the temporary minimum height from the code page.
        _transitionHost.ClearValue(Layoutable.HeightProperty);
        if (selectedIndex == 0)
            _transitionHost.MinHeight = 0d;
        _codeBlock?.ClearValue(Layoutable.HeightProperty);
        _transitionHost.UpdateLayout();

        var minimumHeight = Math.Max(PreviewMinHeight, _previewHeight);
        _transitionHost.MinHeight = minimumHeight;
        var targetHeight = minimumHeight;
        if (selectedIndex is 1 or 2)
            targetHeight = Math.Max(targetHeight, _codeBlock?.DesiredSize.Height ?? 0d);

        if (startHeight <= 0 || Math.Abs(startHeight - targetHeight) < 0.5)
        {
            if (selectedIndex == 0)
                _transitionHost.MinHeight = PreviewMinHeight;
            cancellation.Dispose();
            if (ReferenceEquals(_heightAnimationCancellation, cancellation))
                _heightAnimationCancellation = null;
            return;
        }

        // Keep the final height as the base value so removing the animation cannot
        // restore the expanded height after a reverse transition.
        _transitionHost.Height = targetHeight;
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(300),
            Easing = new SukiEaseOutBack { BounceIntensity = EasingIntensity.Soft },
            FillMode = FillMode.None,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters = { new Setter(Layoutable.HeightProperty, startHeight) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(Layoutable.HeightProperty, targetHeight) }
                }
            }
        };

        try
        {
            await animation.RunAsync(_transitionHost, cancellation.Token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(_heightAnimationCancellation, cancellation))
            {
                _transitionHost.ClearValue(Layoutable.HeightProperty);
                if (selectedIndex == 0)
                    _transitionHost.MinHeight = PreviewMinHeight;
                _heightAnimationCancellation = null;
            }
            cancellation.Dispose();
        }
    }

    private void CancelHeightAnimation()
    {
        _heightAnimationCancellation?.Cancel();
        _heightAnimationCancellation?.Dispose();
        _heightAnimationCancellation = null;
    }

    private static int NormalizeIndex(int value) => Math.Clamp(value, 0, 2);

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

using System.Collections.Specialized;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
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

    public static readonly StyledProperty<Orientation> SplitOrientationProperty =
        AvaloniaProperty.Register<ControlShowcase, Orientation>(
            nameof(SplitOrientation), Orientation.Vertical);

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

    public static readonly DirectProperty<ControlShowcase, IReadOnlyList<CodeBlock>> CodeBlockControlsProperty =
        AvaloniaProperty.RegisterDirect<ControlShowcase, IReadOnlyList<CodeBlock>>(
            nameof(CodeBlockControls), control => control.CodeBlockControls);

    private TabStrip? _tabStrip;
    private TransitioningContentControl? _transitionHost;
    private ContentPresenter? _previewPresenter;
    private Grid? _codePage;
    private CodeBlock? _codeBlock;
    private IReadOnlyList<CodeBlock> _codeBlockControls = Array.Empty<CodeBlock>();
    private readonly HashSet<ShowcaseCodeBlock> _subscribedCodeBlocks = [];
    private Grid? _combinedPage;
    private Border? _combinedDivider;
    private string _effectiveCode = string.Empty;
    private int _displayedIndex;
    private bool _synchronizingSelection;
    private double _previewHeight;
    private CancellationTokenSource? _heightAnimationCancellation;
    private CancellationTokenSource? _splitAnimationCancellation;

    public ControlShowcase()
    {
        CodeBlocks.CollectionChanged += OnCodeBlocksCollectionChanged;
    }

    /// <summary>
    /// Gets the code block definitions. When empty, the legacy single-block properties are used.
    /// </summary>
    public AvaloniaList<ShowcaseCodeBlock> CodeBlocks { get; } = [];

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

    /// <summary>
    /// Gets or sets whether the split page places preview and code top-to-bottom or side-by-side.
    /// </summary>
    public Orientation SplitOrientation
    {
        get => GetValue(SplitOrientationProperty);
        set => SetValue(SplitOrientationProperty, value);
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

    /// <summary>
    /// Gets the code block controls created after the template has been applied.
    /// </summary>
    public IReadOnlyList<CodeBlock> CodeBlockControls => _codeBlockControls;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        CancelHeightAnimation();
        CancelSplitAnimation();
        if (_tabStrip is not null)
            _tabStrip.SelectionChanged -= OnTabSelectionChanged;
        if (_previewPresenter is not null)
            _previewPresenter.SizeChanged -= OnPreviewSizeChanged;

        base.OnApplyTemplate(e);
        UpdateHeaderPlacement();

        _tabStrip = e.NameScope.Find<TabStrip>(PART_TabStrip);
        _transitionHost = e.NameScope.Find<TransitioningContentControl>(PART_TransitionHost);
        _combinedPage = null;
        _combinedDivider = null;

        _previewPresenter = new ContentPresenter();
        _previewPresenter.SizeChanged += OnPreviewSizeChanged;

        UpdatePreviewPresenter();
        UpdateEffectiveCode();
        RebuildCodePage();

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
            CancelSplitAnimation();
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
        else if (change.Property == SplitOrientationProperty)
        {
            var startHeight = _transitionHost?.Bounds.Height ?? 0d;
            CancelSplitAnimation();
            UpdateCombinedLayout();
            if (_displayedIndex == 2 && HeightBehavior == ShowcaseHeightBehavior.Animated)
                _ = AnimateContentHeightAsync(2, startHeight);
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
        CancelSplitAnimation();
        TransitionToPage(previousIndex, selectedIndex);

        if (HeightBehavior == ShowcaseHeightBehavior.Animated)
            _ = AnimateContentHeightAsync(selectedIndex, startHeight);
    }

    private object? GetPage(int selectedIndex) => selectedIndex switch
    {
        0 => _previewPresenter,
        1 => _codePage,
        _ => CreateCombinedPage()
    };

    private Grid CreateCombinedPage()
    {
        _combinedDivider = new Border();
        _combinedDivider.Classes.Add("control-showcase-divider");

        var page = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        page.Children.Add(_previewPresenter!);
        page.Children.Add(_combinedDivider);
        page.Children.Add(_codePage!);
        _combinedPage = page;
        UpdateCombinedLayout();
        return page;
    }

    private void UpdateCombinedLayout()
    {
        if (_combinedPage is null || _combinedDivider is null || _previewPresenter is null || _codePage is null)
            return;

        var isVertical = SplitOrientation == Orientation.Vertical;
        _combinedPage.RowDefinitions = new RowDefinitions(isVertical ? "Auto,1,Auto" : "*");
        _combinedPage.ColumnDefinitions = new ColumnDefinitions(isVertical ? "*" : "*,1,*");

        Grid.SetRow(_previewPresenter, 0);
        Grid.SetColumn(_previewPresenter, 0);
        Grid.SetRow(_combinedDivider, isVertical ? 1 : 0);
        Grid.SetColumn(_combinedDivider, isVertical ? 0 : 1);
        Grid.SetRow(_codePage, isVertical ? 2 : 0);
        Grid.SetColumn(_codePage, isVertical ? 0 : 2);
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

        // Split transitions reparent the live controls. Bypass the page cross-fade so the
        // same preview or code is never drawn twice by overlapping outgoing/incoming pages.
        _transitionHost.PageTransition = null;
        _transitionHost.Content = null;
        ResetTransitionState(_previewPresenter);
        ResetTransitionState(_codePage);
        _combinedPage?.Children.Clear();
        _combinedPage = null;
        _combinedDivider = null;
        _transitionHost.Content = GetPage(selectedIndex);
        _transitionHost.PageTransition = PageTransition;
        var animatedElement = selectedIndex == 2
            ? previousIndex == 0 ? (Control?)_codePage : _previewPresenter
            : selectedIndex == 0 ? _previewPresenter : _codePage;
        _ = AnimateSplitElementAsync(animatedElement, ReferenceEquals(animatedElement, _previewPresenter));
    }

    private async Task AnimateSplitElementAsync(Control? element, bool isPreview)
    {
        if (element is null || PageTransition is null)
            return;

        var cancellation = new CancellationTokenSource();
        _splitAnimationCancellation = cancellation;
        var slideTransition = PageTransition as SlideFadePageTransition;
        var distance = (isPreview ? -1d : 1d) * (slideTransition?.Distance ?? 28d);
        var property = SplitOrientation == Orientation.Vertical
            ? TranslateTransform.YProperty
            : TranslateTransform.XProperty;
        var animation = new Animation
        {
            Duration = slideTransition?.Duration ?? TimeSpan.FromMilliseconds(160),
            Easing = slideTransition?.Easing ?? new SukiEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(property, distance),
                        new Setter(Visual.OpacityProperty, 0d)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(property, 0d),
                        new Setter(Visual.OpacityProperty, 1d)
                    }
                }
            }
        };

        try
        {
            await animation.RunAsync(element, cancellation.Token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(_splitAnimationCancellation, cancellation))
            {
                ResetTransitionState(element);
                _splitAnimationCancellation = null;
            }
            cancellation.Dispose();
        }
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

    private void OnCodeBlocksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var definition in _subscribedCodeBlocks)
            definition.PropertyChanged -= OnCodeBlockDefinitionChanged;
        _subscribedCodeBlocks.Clear();
        foreach (var definition in CodeBlocks)
        {
            if (_subscribedCodeBlocks.Add(definition))
                definition.PropertyChanged += OnCodeBlockDefinitionChanged;
        }

        RebuildCodePage();
    }

    private void OnCodeBlockDefinitionChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        var startHeight = _transitionHost?.Bounds.Height ?? 0d;
        UpdateCodeBlock();
        if (_displayedIndex != 0 && HeightBehavior == ShowcaseHeightBehavior.Animated)
            _ = AnimateContentHeightAsync(_displayedIndex, startHeight);
    }

    private void RebuildCodePage()
    {
        var oldCodeBlock = _codeBlock;
        var oldCodeBlockControls = _codeBlockControls;
        var displayedIndex = _displayedIndex;
        var restoreDisplayedPage = _transitionHost?.Content is not null;
        var startHeight = _transitionHost?.Bounds.Height ?? 0d;

        CancelHeightAnimation();
        CancelSplitAnimation();
        _transitionHost?.ClearValue(Layoutable.HeightProperty);
        if (restoreDisplayedPage)
            _transitionHost!.Content = null;
        _combinedPage?.Children.Clear();
        _combinedPage = null;
        _combinedDivider = null;

        var blockCount = Math.Max(1, CodeBlocks.Count);
        var controls = new List<CodeBlock>(blockCount);
        var page = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        for (var index = 0; index < blockCount; index++)
        {
            if (index > 0)
            {
                page.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Pixel));
                var divider = new Border();
                divider.Classes.Add("control-showcase-divider");
                Grid.SetRow(divider, index * 2 - 1);
                page.Children.Add(divider);
            }

            page.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var codeBlock = new CodeBlock
            {
                CornerRadius = new CornerRadius(0),
                HeaderCornerRadius = new CornerRadius(0),
                ShowCopyButton = true
            };
            codeBlock.Classes.Add("control-showcase-code-block");
            Grid.SetRow(codeBlock, index * 2);
            page.Children.Add(codeBlock);
            controls.Add(codeBlock);
        }

        _codePage = page;
        _codeBlockControls = controls;
        _codeBlock = controls[0];
        RaisePropertyChanged(CodeBlockControlsProperty, oldCodeBlockControls, _codeBlockControls);
        RaisePropertyChanged(CodeBlockProperty, oldCodeBlock, _codeBlock);
        UpdateCodeBlock();

        if (restoreDisplayedPage && _transitionHost is not null)
        {
            _transitionHost.Content = GetPage(displayedIndex);
            if (displayedIndex != 0 && HeightBehavior == ShowcaseHeightBehavior.Animated)
                _ = AnimateContentHeightAsync(displayedIndex, startHeight);
        }
    }

    private void UpdateCodeBlock()
    {
        if (_codePage is null)
            return;

        if (CodeBlocks.Count == 0)
        {
            ConfigureCodeBlock(_codeBlock!, EffectiveCode, Language, CodeBlockHeader, ShowLineNumbers, WordWrap);
        }
        else
        {
            for (var index = 0; index < CodeBlocks.Count && index < _codeBlockControls.Count; index++)
            {
                var definition = CodeBlocks[index];
                ConfigureCodeBlock(
                    _codeBlockControls[index],
                    definition.Code ?? EffectiveCode,
                    definition.Language,
                    definition.Header,
                    definition.ShowLineNumbers,
                    definition.WordWrap);
            }
        }

        if (_previewPresenter is { Bounds.Width: > 0, Bounds.Height: > 0 })
            SyncCodeBlockSize(_previewPresenter.Bounds.Size);
        else
            ApplyCodeBlockHeights(null);
    }

    private static void ConfigureCodeBlock(
        CodeBlock codeBlock,
        string code,
        string language,
        object? header,
        bool showLineNumbers,
        bool wordWrap)
    {
        codeBlock.Text = code;
        codeBlock.Language = language;
        codeBlock.Header = header;
        AvaloniaEditor.SetShowLineNumbers(codeBlock, showLineNumbers);
        AvaloniaEditor.SetWordWrap(codeBlock, wordWrap);
        AvaloniaEditor.SetHorizontalScrollBarVisibility(
            codeBlock,
            wordWrap ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto);
        AvaloniaEditor.SetVerticalScrollBarVisibility(codeBlock, ScrollBarVisibility.Auto);
        AvaloniaEditor.SetIsReadOnly(codeBlock, true);
    }

    private void SyncCodeBlockSize(Size previewSize)
    {
        if (_codePage is null || previewSize.Width <= 0 || previewSize.Height <= 0)
            return;

        _codePage.ClearValue(Layoutable.WidthProperty);
        ApplyCodeBlockHeights(previewSize.Height);
    }

    private void ApplyCodeBlockHeights(double? previewHeight)
    {
        if (_codeBlockControls.Count == 0)
            return;

        if (HeightBehavior == ShowcaseHeightBehavior.Stable && previewHeight is > 0)
        {
            var height = Math.Max(78d, (previewHeight.Value - (_codeBlockControls.Count - 1)) / _codeBlockControls.Count);
            foreach (var codeBlock in _codeBlockControls)
                codeBlock.Height = height;
            return;
        }

        for (var index = 0; index < _codeBlockControls.Count; index++)
        {
            var configuredHeight = CodeBlocks.Count == 0
                ? CodeHeight
                : CodeBlocks[index].CodeHeight;
            if (double.IsNaN(configuredHeight) || double.IsInfinity(configuredHeight))
                configuredHeight = CodeHeight;
            _codeBlockControls[index].Height = Math.Max(78d, configuredHeight);
        }
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
        _codePage?.ClearValue(Layoutable.HeightProperty);
        _transitionHost.UpdateLayout();

        var minimumHeight = Math.Max(PreviewMinHeight, _previewHeight);
        _transitionHost.MinHeight = minimumHeight;
        var targetHeight = minimumHeight;
        if (selectedIndex == 1)
            targetHeight = Math.Max(targetHeight, _codePage?.DesiredSize.Height ?? 0d);
        else if (selectedIndex == 2)
            targetHeight = Math.Max(targetHeight, _combinedPage?.DesiredSize.Height ?? 0d);

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
            Easing = new SukiEaseOut(),
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

    private void CancelSplitAnimation()
    {
        _splitAnimationCancellation?.Cancel();
        _splitAnimationCancellation?.Dispose();
        _splitAnimationCancellation = null;
        ResetTransitionState(_previewPresenter);
        ResetTransitionState(_codePage);
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

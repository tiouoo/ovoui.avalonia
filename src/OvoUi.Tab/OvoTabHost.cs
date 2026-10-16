using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using OvoUi.Controls;
using OvoUi.Tab.Internal;

namespace OvoUi.Tab;

[TemplatePart(PartTabList, typeof(SelectionList))]
[TemplatePart(PartTabScrollViewer, typeof(ScrollViewer))]
public class OvoTabHost : TemplatedControl
{
    public const string PartTabList = "PART_TabList";
    public const string PartTabScrollViewer = "PART_TabScrollViewer";

    private readonly ICommand _closeTabCommand;
    private readonly DelegateCommand _newTabCommand;
    private readonly TabDragBehavior _dragBehavior;
    private readonly ObservableCollection<OvoTabItem> _tabs = [];
    private OvoTabDragState _dragState;
    private OvoTabService _tabService = OvoTabService.Shared;
    private bool _isAttached;
    private SelectionList? _tabList;
    private ScrollViewer? _tabScrollViewer;

    public OvoTabHost()
    {
        _tabs.CollectionChanged += TabsOnCollectionChanged;
        _newTabCommand = new DelegateCommand(RequestNewTab);
        _closeTabCommand = new DelegateCommand<OvoTabItem>(tab => _ = CloseTabAsync(tab));
        _dragBehavior = new TabDragBehavior(this);
    }

    public static readonly DirectProperty<OvoTabHost, ObservableCollection<OvoTabItem>> TabsProperty =
        AvaloniaProperty.RegisterDirect<OvoTabHost, ObservableCollection<OvoTabItem>>(
            nameof(Tabs), host => host.Tabs);

    public ObservableCollection<OvoTabItem> Tabs => _tabs;

    public static readonly DirectProperty<OvoTabHost, OvoTabDragState> DragStateProperty =
        AvaloniaProperty.RegisterDirect<OvoTabHost, OvoTabDragState>(nameof(DragState), host => host.DragState);

    public OvoTabDragState DragState
    {
        get => _dragState;
        private set => SetAndRaise(DragStateProperty, ref _dragState, value);
    }

    public string HostId { get; } = Guid.NewGuid().ToString("N")[..6];

    public static readonly StyledProperty<OvoTabItem?> SelectedTabProperty =
        AvaloniaProperty.Register<OvoTabHost, OvoTabItem?>(nameof(SelectedTab), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public OvoTabItem? SelectedTab
    {
        get => GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    public static readonly StyledProperty<IPageTransition?> PageTransitionProperty =
        AvaloniaProperty.Register<OvoTabHost, IPageTransition?>(nameof(PageTransition),
            new CrossFade(TimeSpan.FromMilliseconds(200)));

    public IPageTransition? PageTransition
    {
        get => GetValue(PageTransitionProperty);
        set => SetValue(PageTransitionProperty, value);
    }

    public static readonly StyledProperty<OvoTabDragMode> DragModeProperty =
        AvaloniaProperty.Register<OvoTabHost, OvoTabDragMode>(nameof(DragMode), OvoTabDragMode.Automatic);

    public OvoTabDragMode DragMode
    {
        get => GetValue(DragModeProperty);
        set => SetValue(DragModeProperty, value);
    }

    public static readonly StyledProperty<bool> IsAddButtonVisibleProperty =
        AvaloniaProperty.Register<OvoTabHost, bool>(nameof(IsAddButtonVisible), true);

    public bool IsAddButtonVisible
    {
        get => GetValue(IsAddButtonVisibleProperty);
        set => SetValue(IsAddButtonVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> IsTabStripVisibleProperty =
        AvaloniaProperty.Register<OvoTabHost, bool>(nameof(IsTabStripVisible), true);

    public bool IsTabStripVisible
    {
        get => GetValue(IsTabStripVisibleProperty);
        set => SetValue(IsTabStripVisibleProperty, value);
    }

    public static readonly StyledProperty<double> TabStripHeightProperty =
        AvaloniaProperty.Register<OvoTabHost, double>(nameof(TabStripHeight), 44,
            coerce: (_, value) => Math.Max(0, value));

    public double TabStripHeight
    {
        get => GetValue(TabStripHeightProperty);
        set => SetValue(TabStripHeightProperty, value);
    }

    public static readonly StyledProperty<Thickness> TabStripPaddingProperty =
        AvaloniaProperty.Register<OvoTabHost, Thickness>(nameof(TabStripPadding), new Thickness(3, 0));

    public Thickness TabStripPadding
    {
        get => GetValue(TabStripPaddingProperty);
        set => SetValue(TabStripPaddingProperty, value);
    }

    public static readonly StyledProperty<object?> LeadingHeaderContentProperty =
        AvaloniaProperty.Register<OvoTabHost, object?>(nameof(LeadingHeaderContent));

    public object? LeadingHeaderContent
    {
        get => GetValue(LeadingHeaderContentProperty);
        set => SetValue(LeadingHeaderContentProperty, value);
    }

    public static readonly StyledProperty<object?> TrailingHeaderContentProperty =
        AvaloniaProperty.Register<OvoTabHost, object?>(nameof(TrailingHeaderContent));

    public object? TrailingHeaderContent
    {
        get => GetValue(TrailingHeaderContentProperty);
        set => SetValue(TrailingHeaderContentProperty, value);
    }

    public static readonly StyledProperty<object?> EmptyContentProperty =
        AvaloniaProperty.Register<OvoTabHost, object?>(nameof(EmptyContent));

    public object? EmptyContent
    {
        get => GetValue(EmptyContentProperty);
        set => SetValue(EmptyContentProperty, value);
    }

    public static readonly StyledProperty<OvoTabStrings> StringsProperty =
        AvaloniaProperty.Register<OvoTabHost, OvoTabStrings>(nameof(Strings), OvoTabStrings.English);

    public OvoTabStrings Strings
    {
        get => GetValue(StringsProperty);
        set => SetValue(StringsProperty, value ?? throw new ArgumentNullException(nameof(value)));
    }

    public OvoTabService TabService
    {
        get => _tabService;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (ReferenceEquals(_tabService, value)) return;
            if (_isAttached) _tabService.Unregister(this);
            _tabService = value;
            if (_isAttached) _tabService.Register(this);
        }
    }

    public Func<OvoTabItem?>? NewTabFactory { get; set; }

    public Func<OvoTabClosingContext, CancellationToken, ValueTask<bool>>? CloseGuard { get; set; }

    public ICommand NewTabCommand => _newTabCommand;

    public ICommand CloseTabCommand => _closeTabCommand;

    public bool IsPointerDragSupported =>
        !OperatingSystem.IsMacOS() &&
        !OperatingSystem.IsBrowser() &&
        !OperatingSystem.IsAndroid() &&
        !OperatingSystem.IsIOS() &&
        TopLevel.GetTopLevel(this) is Window;

    public OvoTabDragMode EffectiveDragMode => !IsPointerDragSupported
        ? OvoTabDragMode.Disabled
        : DragMode == OvoTabDragMode.Automatic
            ? OvoTabDragMode.ReorderAndDetach
            : DragMode;

    public event EventHandler<OvoTabEventArgs>? TabAdded;
    public event EventHandler<OvoTabClosingEventArgs>? TabClosing;
    public event EventHandler<OvoTabEventArgs>? TabClosed;
    public event EventHandler<OvoTabEventArgs>? SelectedTabChanged;
    public event EventHandler<OvoTabNewRequestedEventArgs>? NewTabRequested;
    public event EventHandler<OvoTabMenuOpeningEventArgs>? TabMenuOpening;
    public event EventHandler<OvoTabDragStateChangedEventArgs>? DragStateChanged;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();
        base.OnApplyTemplate(e);

        _tabList = e.NameScope.Find<SelectionList>(PartTabList);
        _tabScrollViewer = e.NameScope.Find<ScrollViewer>(PartTabScrollViewer);
        AttachTemplateHandlers();
        if (Tabs.Count > 0) ScrollToEnd();
    }

    private void AttachTemplateHandlers()
    {
        DetachTemplateHandlers();
        if (!this.IsAttachedToVisualTree()) return;
        if (_tabList is not null)
        {
            _tabList.AddHandler(PointerPressedEvent, TabListOnPointerPressed, RoutingStrategies.Tunnel,
                handledEventsToo: true);
            _dragBehavior.Attach(_tabList);
        }

        if (_tabScrollViewer is not null)
            _tabScrollViewer.PointerWheelChanged += TabScrollViewerOnPointerWheelChanged;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        TabService.Register(this);
        AttachTemplateHandlers();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        _dragBehavior.Detach();
        TabService.Unregister(this);
        DetachTemplateHandlers();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != SelectedTabProperty) return;

        var tab = change.GetNewValue<OvoTabItem?>();
        if (tab is not null && !Tabs.Contains(tab))
        {
            SetCurrentValue(SelectedTabProperty, Tabs.FirstOrDefault());
            return;
        }

        if (tab is not null)
            SelectedTabChanged?.Invoke(this, new OvoTabEventArgs(tab));
    }

    public void AddTab(OvoTabItem tab, bool select = true)
    {
        ArgumentNullException.ThrowIfNull(tab);
        if (Tabs.Contains(tab))
        {
            if (select) SelectedTab = tab;
            return;
        }

        Tabs.Add(tab);
        if (select) SelectedTab = tab;
        TabAdded?.Invoke(this, new OvoTabEventArgs(tab));
        ScrollToEnd();
    }

    public bool SelectTab(OvoTabItem tab)
    {
        if (!Tabs.Contains(tab)) return false;
        SelectedTab = tab;
        return true;
    }

    public bool MoveTab(OvoTabItem tab, int newIndex)
    {
        var oldIndex = Tabs.IndexOf(tab);
        if (oldIndex < 0 || newIndex < 0 || newIndex >= Tabs.Count || oldIndex == newIndex) return false;
        Tabs.Move(oldIndex, newIndex);
        SelectedTab = tab;
        return true;
    }

    public async Task<bool> CloseTabAsync(OvoTabItem tab, OvoTabCloseReason reason = OvoTabCloseReason.User,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tab);
        if (!Tabs.Contains(tab) || (reason == OvoTabCloseReason.User && !tab.IsCloseable) || !tab.TryBeginClose())
            return false;

        try
        {
            if (!await CanCloseAsync(tab, reason, cancellationToken)) return false;
            RemoveClosedTab(tab, reason);
            return true;
        }
        finally
        {
            tab.EndClose();
        }
    }

    public async Task<bool> CloseOtherTabsAsync(OvoTabItem tab, CancellationToken cancellationToken = default)
    {
        if (!Tabs.Contains(tab)) return false;
        var candidates = Tabs.Where(item => !ReferenceEquals(item, tab) && item.IsCloseable).ToArray();
        foreach (var candidate in candidates)
        {
            if (!candidate.TryBeginClose()) return false;
            try
            {
                if (!await CanCloseAsync(candidate, OvoTabCloseReason.CloseOthers, cancellationToken)) return false;
            }
            finally
            {
                candidate.EndClose();
            }
        }

        foreach (var candidate in candidates)
            RemoveClosedTab(candidate, OvoTabCloseReason.CloseOthers);
        SelectedTab = tab;
        return true;
    }

    /// <summary>
    /// Closes a closeable tab without invoking close guards or <see cref="TabClosing"/>.
    /// </summary>
    public bool CloseTabImmediately(OvoTabItem tab, OvoTabCloseReason reason = OvoTabCloseReason.Programmatic)
    {
        ArgumentNullException.ThrowIfNull(tab);
        if (!tab.IsCloseable || !Tabs.Contains(tab) || !tab.TryBeginClose()) return false;

        try
        {
            RemoveClosedTab(tab, reason);
            return true;
        }
        finally
        {
            tab.EndClose();
        }
    }

    public async Task<bool> CloseAllTabsAsync(CancellationToken cancellationToken = default)
    {
        var candidates = Tabs.Where(tab => tab.IsCloseable).ToArray();
        foreach (var candidate in candidates)
        {
            if (!await CanCloseAsync(candidate, OvoTabCloseReason.Programmatic, cancellationToken)) return false;
        }

        foreach (var candidate in candidates)
            RemoveClosedTab(candidate, OvoTabCloseReason.Programmatic);
        return true;
    }

    internal async Task<bool> CloseAllForWindowAsync(CancellationToken cancellationToken = default)
    {
        var candidates = Tabs.ToArray();
        foreach (var candidate in candidates)
        {
            if (!await CanCloseAsync(candidate, OvoTabCloseReason.WindowClosing, cancellationToken)) return false;
        }

        foreach (var candidate in candidates)
            RemoveClosedTab(candidate, OvoTabCloseReason.WindowClosing);
        return true;
    }

    internal void RemoveForTransfer(OvoTabItem tab)
    {
        var index = Tabs.IndexOf(tab);
        if (index < 0) return;
        if (ReferenceEquals(SelectedTab, tab))
            SelectedTab = Tabs.LastOrDefault(item => !ReferenceEquals(item, tab));
        Tabs.RemoveAt(index);
    }

    internal void UpdateDragState(OvoTabDragState state)
    {
        if (DragState == state) return;
        var oldState = DragState;
        DragState = state;
        DragStateChanged?.Invoke(this, new OvoTabDragStateChangedEventArgs(oldState, state));
    }

    public MenuFlyout BuildTabMenu(OvoTabItem tab)
    {
        ArgumentNullException.ThrowIfNull(tab);
        var flyout = new MenuFlyout();

        if (tab.IsCloseable)
            flyout.Items.Add(MenuItem(Strings.CloseTab, () => _ = CloseTabAsync(tab), OvoTabIcons.Close, "Ctrl+W",
                iconWidth: 11));

        if (Tabs.Count > 1)
        {
            flyout.Items.Add(MenuItem(Strings.CloseOtherTabs, () => _ = CloseOtherTabsAsync(tab)));

            var reorder = new MenuItem { Header = Strings.ReorderTabs, Icon = Icon(OvoTabIcons.Reorder, width: 15) };
            var index = Tabs.IndexOf(tab);
            reorder.Items.Add(MenuItem(Strings.MoveBackward, () => MoveTab(tab, index - 1), isEnabled: index > 0));
            reorder.Items.Add(MenuItem(Strings.MoveForward, () => MoveTab(tab, index + 1), isEnabled: index < Tabs.Count - 1));
            reorder.Items.Add(MenuItem(Strings.MoveToStart, () => MoveTab(tab, 0), isEnabled: index > 0));
            reorder.Items.Add(MenuItem(Strings.MoveToEnd, () => MoveTab(tab, Tabs.Count - 1),
                isEnabled: index < Tabs.Count - 1));
            flyout.Items.Add(reorder);
        }

        var otherHosts = TabService.Hosts.Where(host => !ReferenceEquals(host, this) &&
                                                         TopLevel.GetTopLevel(host) is Window).ToArray();
        if (otherHosts.Length > 0)
        {
            var move = new MenuItem
            {
                Header = Strings.MoveToWindow,
                Icon = Icon(OvoTabIcons.Transfer, width: 17, height: 17, margin: new Thickness(1, 0, 0, 0))
            };
            foreach (var target in otherHosts)
            {
                var window = TopLevel.GetTopLevel(target) as Window;
                move.Items.Add(MenuItem(window?.Title ?? target.HostId, () => TabService.Transfer(tab, this, target)));
            }
            flyout.Items.Add(move);
        }

        if (Tabs.Count > 1 && EffectiveDragMode == OvoTabDragMode.ReorderAndDetach && TabService.WindowFactory is not null)
            flyout.Items.Add(MenuItem(Strings.OpenInNewWindow,
                () => TabService.Detach(tab, this, GetDefaultDetachPosition()), OvoTabIcons.Detach,
                iconWidth: 18, iconHeight: 18, iconMargin: new Thickness(1, 0, 0, 0)));

        var additionalItems = new List<MenuItem>();
        if (tab.Content is IOvoTabMenuProvider provider)
        {
            provider.PopulateTabMenu(additionalItems, new OvoTabMenuContext(this, tab));
        }

        TabMenuOpening?.Invoke(this, new OvoTabMenuOpeningEventArgs(tab, additionalItems));
        if (additionalItems.Count > 0)
        {
            flyout.Items.Add(new Separator());
            foreach (var item in additionalItems) flyout.Items.Add(item);
        }
        return flyout;
    }

    internal void ShowTabMenu(OvoTabItem tab, Control placementTarget)
    {
        BuildTabMenu(tab).ShowAt(placementTarget);
    }

    private async ValueTask<bool> CanCloseAsync(OvoTabItem tab, OvoTabCloseReason reason,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var eventArgs = new OvoTabClosingEventArgs(tab, reason);
        TabClosing?.Invoke(this, eventArgs);
        if (eventArgs.Cancel) return false;

        var context = new OvoTabClosingContext(tab, reason);
        if (CloseGuard is not null && !await CloseGuard(context, cancellationToken)) return false;
        return tab.Content is not IOvoTabPage page || await page.OnClosingAsync(context, cancellationToken);
    }

    private void RemoveClosedTab(OvoTabItem tab, OvoTabCloseReason reason)
    {
        var index = Tabs.IndexOf(tab);
        if (index < 0) return;
        var wasSelected = ReferenceEquals(SelectedTab, tab);

        // Move selection first. Selection controls can otherwise write the removed item
        // back through their two-way binding while processing CollectionChanged.
        if (wasSelected)
            SelectedTab = Tabs.LastOrDefault(item => !ReferenceEquals(item, tab));

        Tabs.RemoveAt(index);

        if (tab.Content is IOvoTabPage page)
            page.OnClosed(new OvoTabClosedContext(tab, reason));
        TabClosed?.Invoke(this, new OvoTabEventArgs(tab));
    }

    private void RequestNewTab()
    {
        var args = new OvoTabNewRequestedEventArgs { Tab = NewTabFactory?.Invoke() };
        NewTabRequested?.Invoke(this, args);
        if (args.Tab is not null) AddTab(args.Tab);
    }

    private void TabsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Tabs.Count == 0)
        {
            SelectedTab = null;
            return;
        }

        if (SelectedTab is null || !Tabs.Contains(SelectedTab))
            SelectedTab = Tabs[0];
    }

    private void TabListOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (!point.Properties.IsRightButtonPressed && !point.Properties.IsMiddleButtonPressed) return;
        if (FindTabRoot(e.Source as Visual) is not { Tag: OvoTabItem tab } root) return;

        if (point.Properties.IsRightButtonPressed)
            ShowTabMenu(tab, root);
        else if (tab.IsCloseable)
            _ = CloseTabAsync(tab);
        e.Handled = true;
    }

    private void TabScrollViewerOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_tabScrollViewer is null || Math.Abs(e.Delta.Y) < double.Epsilon) return;
        _tabScrollViewer.Offset = new Vector(
            Math.Max(0, _tabScrollViewer.Offset.X - e.Delta.Y * 20),
            _tabScrollViewer.Offset.Y);
        e.Handled = true;
    }

    private void ScrollToEnd()
    {
        if (_tabScrollViewer is null) return;
        // Selection queues BringIntoView during layout. Scroll past it to include the add button.
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (_tabScrollViewer is not null && this.IsAttachedToVisualTree())
                _tabScrollViewer.Offset = new Vector(double.PositiveInfinity, _tabScrollViewer.Offset.Y);
        }, Avalonia.Threading.DispatcherPriority.Background);
    }

    private PixelPoint GetDefaultDetachPosition()
    {
        if (TopLevel.GetTopLevel(this) is Window window)
            return new PixelPoint(window.Position.X + 80, window.Position.Y + 40);
        return new PixelPoint(100, 100);
    }

    private void DetachTemplateHandlers()
    {
        _dragBehavior.Detach();
        if (_tabList is not null)
            _tabList.RemoveHandler(PointerPressedEvent, TabListOnPointerPressed);
        if (_tabScrollViewer is not null)
            _tabScrollViewer.PointerWheelChanged -= TabScrollViewerOnPointerWheelChanged;
    }

    internal static Border? FindTabRoot(Visual? visual)
    {
        while (visual is not null)
        {
            if (visual is Border border && border.Classes.Contains("tab-root")) return border;
            visual = visual.GetVisualParent();
        }
        return null;
    }

    private static MenuItem MenuItem(string header, Action action, Avalonia.Media.StreamGeometry? icon = null,
        string? gesture = null, bool isEnabled = true, double iconWidth = 16, double iconHeight = 16,
        Thickness iconMargin = default) => new()
    {
        Header = header,
        Command = new DelegateCommand(action),
        Icon = icon is null ? null : Icon(icon, iconWidth, iconHeight, iconMargin),
        InputGesture = gesture is null ? null : KeyGesture.Parse(gesture),
        IsEnabled = isEnabled
    };

    private static PathIcon Icon(Avalonia.Media.StreamGeometry geometry, double width = 16, double height = 16,
        Thickness margin = default) => new()
    {
        Data = geometry,
        Width = width,
        Height = height,
        Margin = margin,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
    };
}

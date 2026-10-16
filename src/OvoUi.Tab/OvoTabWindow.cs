using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using OvoUi.Controls;
using OvoUi.Tab.Internal;

namespace OvoUi.Tab;

/// <summary>
/// Desktop window shell for <see cref="OvoTabHost"/>. Use the host directly on single-view platforms.
/// </summary>
public class OvoTabWindow : OvoWindow
{
    private readonly DelegateCommand _closeAllCommand;
    private readonly DelegateCommand _closeCurrentCommand;
    private readonly DelegateCommand _newTabCommand;
    private bool _closeApproved;
    private bool _isClosingWindow;

    static OvoTabWindow()
    {
        TabStripRightInsetProperty.Changed.AddClassHandler<OvoTabWindow>((window, _) =>
            window.UpdateTabStripWidth(window.ClientSize.Width));
    }

    public OvoTabWindow() : this(OvoTabService.Shared)
    {
    }

    public OvoTabWindow(OvoTabService tabService)
    {
        ArgumentNullException.ThrowIfNull(tabService);
        WindowId = Guid.NewGuid().ToString("N")[..6];
        TabHost = new OvoTabHost { TabService = tabService, IsTabStripVisible = false };
        TabHost.TabStripPadding = OperatingSystem.IsMacOS()
            ? new Avalonia.Thickness(92, 3, 12, 3)
            : new Avalonia.Thickness(6, 3);
        TabStrip = new OvoTabStrip
        {
            Host = TabHost,
            Height = 44,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
        };
        OvoTitleBar.SetLeadingContent(this, TabStrip);
        Content = TabHost;
        Width = 1100;
        Height = 700;
        MinWidth = 640;
        MinHeight = 420;
        OvoTitleBar.SetTitleBarHeight(this, 44);

        if (OperatingSystem.IsMacOS())
        {
            TabHost.DragMode = OvoTabDragMode.Disabled;
            OvoTitleBar.SetIsCloseButtonVisible(this, false);
            OvoTitleBar.SetIsMinimizeButtonVisible(this, false);
            OvoTitleBar.SetIsMaximizeButtonVisible(this, false);
            OvoTitleBar.SetIsFullScreenButtonVisible(this, false);
        }

        _newTabCommand = new DelegateCommand(() => TabHost.NewTabCommand.Execute(null));
        _closeCurrentCommand = new DelegateCommand(() =>
        {
            if (TabHost.SelectedTab is { } selected) _ = TabHost.CloseTabAsync(selected);
        });
        _closeAllCommand = new DelegateCommand(() => _ = TabHost.CloseAllTabsAsync());

        KeyBindings.Add(new KeyBinding { Gesture = KeyGesture.Parse("Ctrl+T"), Command = _newTabCommand });
        KeyBindings.Add(new KeyBinding { Gesture = KeyGesture.Parse("Ctrl+W"), Command = _closeCurrentCommand });
        KeyBindings.Add(new KeyBinding { Gesture = KeyGesture.Parse("Ctrl+Shift+W"), Command = _closeAllCommand });

        Closing += OnWindowClosing;
        TabHost.SelectedTabChanged += OnSelectedTabChanged;
        TabHost.TabClosed += OnTabClosed;
    }

    public string WindowId { get; }

    public static readonly Avalonia.StyledProperty<bool> IsTabStripWindowDragEnabledProperty =
        Avalonia.AvaloniaProperty.Register<OvoTabWindow, bool>(nameof(IsTabStripWindowDragEnabled), true);

    /// <summary>
    /// Allows the tab strip to move this window when tab manipulation is unavailable or unnecessary.
    /// </summary>
    public bool IsTabStripWindowDragEnabled
    {
        get => GetValue(IsTabStripWindowDragEnabledProperty);
        set => SetValue(IsTabStripWindowDragEnabledProperty, value);
    }

    public OvoTabHost TabHost { get; }

    public OvoTabStrip TabStrip { get; }

    public static readonly Avalonia.StyledProperty<double> TabStripRightInsetProperty =
        Avalonia.AvaloniaProperty.Register<OvoTabWindow, double>(nameof(TabStripRightInset), 190,
            coerce: (_, value) => Math.Max(0, value));

    /// <summary>
    /// Space kept outside the interactive tab strip for window controls and a draggable title-bar region.
    /// Changes take effect immediately. On macOS the native traffic-light inset is handled separately.
    /// </summary>
    public double TabStripRightInset
    {
        get => GetValue(TabStripRightInsetProperty);
        set => SetValue(TabStripRightInsetProperty, value);
    }

    public OvoTabService TabService
    {
        get => TabHost.TabService;
        set => TabHost.TabService = value;
    }

    public Func<OvoTabItem?>? NewTabFactory
    {
        get => TabHost.NewTabFactory;
        set => TabHost.NewTabFactory = value;
    }

    public bool IsPrimaryWindow { get; set; }

    public bool CreateTabWhenEmpty { get; set; } = true;

    public bool UseSelectedTabTitle { get; set; } = true;

    public void AddTab(OvoTabItem tab, bool select = true) => TabHost.AddTab(tab, select);

    public Task<bool> CloseTabAsync(OvoTabItem tab, CancellationToken cancellationToken = default) =>
        TabHost.CloseTabAsync(tab, OvoTabCloseReason.User, cancellationToken);

    internal void CloseAfterTransfer()
    {
        _closeApproved = true;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        Closing -= OnWindowClosing;
        TabHost.SelectedTabChanged -= OnSelectedTabChanged;
        TabHost.TabClosed -= OnTabClosed;
        base.OnClosed(e);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateTabStripWidth(e.NewSize.Width);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        UpdateTabStripWidth(ClientSize.Width);
    }

    private void UpdateTabStripWidth(double clientWidth)
    {
        var inset = OperatingSystem.IsMacOS() ? 12 : TabStripRightInset;
        TabStrip.Width = Math.Max(0, clientWidth - inset);
    }

    private async void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_closeApproved || _isClosingWindow) return;
        e.Cancel = true;
        _isClosingWindow = true;
        try
        {
            if (!await TabHost.CloseAllForWindowAsync()) return;
            _closeApproved = true;
            Close();
        }
        finally
        {
            _isClosingWindow = false;
        }
    }

    private void OnSelectedTabChanged(object? sender, OvoTabEventArgs e)
    {
        if (UseSelectedTabTitle) Title = e.Tab.Title;
    }

    private void OnTabClosed(object? sender, OvoTabEventArgs e)
    {
        if (_isClosingWindow || !CreateTabWhenEmpty || TabHost.Tabs.Count != 0 || NewTabFactory is null) return;
        if (NewTabFactory() is { } tab) TabHost.AddTab(tab);
    }
}

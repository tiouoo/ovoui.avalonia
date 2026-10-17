using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using OvoUi.Controls;
using OvoUi.Tab.Internal;

namespace OvoUi.Tab;

/// <summary>
/// A standalone visual strip for an <see cref="OvoTabHost"/>, useful in custom window chrome.
/// </summary>
[TemplatePart(OvoTabHost.PartTabList, typeof(SelectionList))]
[TemplatePart(OvoTabHost.PartTabScrollViewer, typeof(ScrollViewer))]
public sealed class OvoTabStrip : TemplatedControl
{
    private TabDragBehavior? _dragBehavior;
    private SelectionList? _tabList;
    private ScrollViewer? _scrollViewer;
    private OvoTabHost? _subscribedHost;

    static OvoTabStrip()
    {
        HostProperty.Changed.AddClassHandler<OvoTabStrip>((strip, _) => strip.AttachHandlers());
    }

    public static readonly StyledProperty<OvoTabHost?> HostProperty =
        AvaloniaProperty.Register<OvoTabStrip, OvoTabHost?>(nameof(Host));

    public OvoTabHost? Host
    {
        get => GetValue(HostProperty);
        set => SetValue(HostProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachHandlers();
        base.OnApplyTemplate(e);

        _tabList = e.NameScope.Find<SelectionList>(OvoTabHost.PartTabList);
        _scrollViewer = e.NameScope.Find<ScrollViewer>(OvoTabHost.PartTabScrollViewer);
        AttachHandlers();
        if (Host?.Tabs.Count > 0) ScrollToEnd();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachHandlers();
    }

    private void AttachHandlers()
    {
        DetachHandlers();
        if (!this.IsAttachedToVisualTree()) return;
        if (_tabList is not null)
        {
            _tabList.AddHandler(PointerPressedEvent, OnPointerPressed,
                RoutingStrategies.Tunnel, handledEventsToo: true);
        }

        if (_scrollViewer is not null)
            _scrollViewer.PointerWheelChanged += OnPointerWheelChanged;

        _subscribedHost = Host;
        if (_subscribedHost is not null)
        {
            _subscribedHost.TabAdded += OnTabAdded;
            if (_tabList is not null) _subscribedHost.RegisterHeaderList(_tabList);
        }

        AttachDragBehavior();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DetachHandlers();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Host is null) return;
        var point = e.GetCurrentPoint(this);
        if (!point.Properties.IsRightButtonPressed && !point.Properties.IsMiddleButtonPressed) return;
        if (OvoTabHost.FindTabRoot(e.Source as Visual) is not { Tag: OvoTabItem tab } root) return;

        if (point.Properties.IsRightButtonPressed)
            Host.ShowTabMenu(tab, root);
        else if (tab.IsCloseable)
            _ = Host.CloseTabAsync(tab);
        e.Handled = true;
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_scrollViewer is null || Math.Abs(e.Delta.Y) < double.Epsilon) return;
        _scrollViewer.Offset = new Vector(
            Math.Max(0, _scrollViewer.Offset.X - e.Delta.Y * 20),
            _scrollViewer.Offset.Y);
        e.Handled = true;
    }

    private void AttachDragBehavior()
    {
        _dragBehavior?.Detach();
        _dragBehavior = null;
        if (Host is null || _tabList is null) return;

        _dragBehavior = new TabDragBehavior(Host);
        _dragBehavior.Attach(_tabList);
    }

    private void OnTabAdded(object? sender, OvoTabEventArgs e)
    {
        if (Host?.TabService.ActiveDrag is null) ScrollToEnd();
    }

    private void ScrollToEnd()
    {
        // Selection queues BringIntoView during layout. Scroll past it to include the add button.
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (_scrollViewer is not null && this.IsAttachedToVisualTree())
                _scrollViewer.Offset = new Vector(double.PositiveInfinity, _scrollViewer.Offset.Y);
        }, Avalonia.Threading.DispatcherPriority.Background);
    }

    private void DetachHandlers()
    {
        _dragBehavior?.Detach();
        _dragBehavior = null;
        if (_subscribedHost is not null)
        {
            _subscribedHost.TabAdded -= OnTabAdded;
            if (_tabList is not null) _subscribedHost.UnregisterHeaderList(_tabList);
        }

        _subscribedHost = null;
        if (_tabList is not null) _tabList.RemoveHandler(PointerPressedEvent, OnPointerPressed);
        if (_scrollViewer is not null) _scrollViewer.PointerWheelChanged -= OnPointerWheelChanged;
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace OvoUi.Tab.Internal;

/// <summary>
/// Owns capture on the original top-level, independently of tab containers and current host.
/// No capturing window is hidden or closed until the pointer has been released.
/// </summary>
internal sealed class TabDragSession
{
    private const double DragThreshold = 5;
    private readonly OvoTabService _service;
    private readonly OvoTabItem _tab;
    private readonly Window _captureRoot;
    private readonly IPointer _pointer;
    private readonly PixelPoint _start;
    private readonly double _grabOffset;
    private readonly HashSet<OvoTabHost> _touchedHosts = [];
    private readonly Dictionary<Window, (double Opacity, bool ShowActivated, bool Topmost)> _windowSettings = [];
    private readonly DispatcherTimer _scrollTimer;
    private TabDragBehavior _current;
    private OvoTabPanel? _dragPanel;
    private Window? _floatingWindow;
    private OvoTabWindow? _reusableWindow;
    private OvoTabWindow? _createdWindow;
    private Vector _windowGrab;
    private PixelPoint _lastPoint;
    private bool _dragging;
    private bool _finished;
    private bool _transitioning;
    private bool _enteredThroughWindow;

    internal TabDragBehavior Origin { get; }

    internal TabDragSession(TabDragBehavior origin, OvoTabItem tab, Window window, PointerPressedEventArgs e)
    {
        Origin = _current = origin;
        _service = origin.Host.TabService;
        _tab = tab;
        _captureRoot = window;
        _pointer = e.Pointer;
        _start = _lastPoint = ScreenPoint(e);
        var child = origin.Panel?.Children.FirstOrDefault(child => ReferenceEquals(child.DataContext, tab));
        _grabOffset = child is null ? 50 : origin.PanelPoint(_start).X - child.Bounds.X;
        _touchedHosts.Add(origin.Host);
        _scrollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _scrollTimer.Tick += OnScrollTick;
        window.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel, true);
        window.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel, true);
        window.AddHandler(InputElement.PointerCaptureLostEvent, OnCaptureLost,
            RoutingStrategies.Direct | RoutingStrategies.Bubble, true);
        window.Closed += OnCaptureWindowClosed;
        e.Pointer.Capture(window);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_finished || !ReferenceEquals(e.Pointer, _pointer)) return;
        _lastPoint = ScreenPoint(e);
        if (!e.GetCurrentPoint(_captureRoot).Properties.IsLeftButtonPressed)
        {
            Finish();
            return;
        }
        if (!_dragging)
        {
            var distance = _lastPoint - _start;
            if (Math.Sqrt((double)distance.X * distance.X + (double)distance.Y * distance.Y) <=
                DragThreshold * _captureRoot.RenderScaling) return;
            _dragging = true;
            _tab.IsDragging = true;
            _scrollTimer.Start();

            if (_current.Host.Tabs.Count == 1 && _captureRoot is OvoTabWindow
                { IsTabStripWindowDragEnabled: true, WindowState: WindowState.Normal } tabWindow &&
                ReferenceEquals(tabWindow.TabHost, _current.Host))
            {
                _floatingWindow = _reusableWindow = tabWindow;
                RememberWindow(tabWindow);
                tabWindow.Topmost = true;
                _windowGrab = new Vector((_start.X - tabWindow.Position.X) / tabWindow.RenderScaling,
                    (_start.Y - tabWindow.Position.Y) / tabWindow.RenderScaling);
            }
        }
        ContinueDrag(_lastPoint);
        e.Handled = true;
    }

    private void ContinueDrag(PixelPoint point)
    {
        if (_finished || _transitioning) return;
        if (!_current.Host.Tabs.Contains(_tab))
        {
            Finish();
            return;
        }

        var host = _current.Host;
        if (_floatingWindow is not null)
        {
            var target = host.IsTabAttachEnabled && host.EffectiveDragMode == OvoTabDragMode.ReorderAndDetach
                ? _service.FindSurfaceAt(point, host, _floatingWindow)
                : null;
            if (target is not null && AttachTo(target, point)) return;
            MoveFloatingWindow(point);
            SetState(OvoTabDragState.MoveWindow);
            return;
        }

        // The tolerance applies only to leaving a strip; entering a target uses its exact bounds.
        if (_enteredThroughWindow && _current.Contains(point, OvoTabDragBoundary.TabStrip))
            _enteredThroughWindow = false;
        var detachBoundary = _enteredThroughWindow ? OvoTabDragBoundary.Window : host.DetachBoundary;
        if (_current.Contains(point, detachBoundary, host.DetachDistance) ||
            host.EffectiveDragMode != OvoTabDragMode.ReorderAndDetach)
        {
            DragInStrip(point);
            return;
        }

        var attachTarget = host.IsTabAttachEnabled
            ? _service.FindSurfaceAt(point, host, _reusableWindow)
            : null;
        if (attachTarget is not null && AttachTo(attachTarget, point)) return;
        if (!DetachToWindow(point)) DragInStrip(point);
    }

    private void DragInStrip(PixelPoint point)
    {
        if (_current.Panel is not { } panel) return;
        if (!ReferenceEquals(_dragPanel, panel))
        {
            if (!panel.BeginDrag(_tab, _current.PanelPoint(point), _grabOffset)) return;
            _dragPanel = panel;
        }
        panel.DragTo(_current.PanelPoint(point));
        SetState(OvoTabDragState.Reorder);
    }

    private bool AttachTo(TabDragBehavior target, PixelPoint point)
    {
        _transitioning = true;
        try
        {
            var source = _current.Host;
            CommitVisualOrder();
            if (!_service.Transfer(_tab, source, target.Host, target.InsertionIndex(point),
                    closeEmpty: false, activate: false)) return false;
            _touchedHosts.Add(target.Host);
            if (_floatingWindow is { } floating)
            {
                RememberWindow(floating);
                // Hide visually, but keep the native capture window mapped. Hide()/Close()
                // can cancel native capture before the next mouse event reaches this session.
                floating.Opacity = 0;
                floating.Topmost = _windowSettings[floating].Topmost;
                _floatingWindow = null;
            }
            source.UpdateDragState(OvoTabDragState.None);
            _current = target;
            _enteredThroughWindow = target.Host.AttachBoundary == OvoTabDragBoundary.Window;
            (TopLevel.GetTopLevel(target.Container!) as Window)?.Activate();
            target.Container?.UpdateLayout();
            DragInStrip(point);
            SetState(OvoTabDragState.Transfer);
            return true;
        }
        finally
        {
            _transitioning = false;
        }
    }

    private bool DetachToWindow(PixelPoint point)
    {
        var source = _current.Host;
        if (_service.WindowFactory is null) return false;
        _transitioning = true;
        try
        {
            var window = _reusableWindow;
            if (window is null)
            {
                window = _service.CreateDetachedWindow(_tab, source, point);
                if (window is null) return false;
                _createdWindow = _reusableWindow = window;
            }
            CommitVisualOrder();
            RememberWindow(window);
            window.ShowActivated = false;
            window.Topmost = true;
            window.Position = new PixelPoint(point.X - (int)Math.Round((_grabOffset + 6) * window.RenderScaling),
                point.Y - (int)Math.Round(22 * window.RenderScaling));
            if (!_service.Transfer(_tab, source, window.TabHost, 0, closeEmpty: false, activate: false)) return false;
            window.Opacity = _windowSettings[window].Opacity;
            if (!window.IsVisible) window.Show();
            window.UpdateLayout();
            var surface = window.TabHost.DragSurfaces.FirstOrDefault(candidate => candidate.Panel is not null);
            if (surface?.Panel is { } panel)
            {
                var tabOrigin = panel.PointToScreen(default);
                _windowGrab = new Vector((tabOrigin.X - window.Position.X) / window.RenderScaling + _grabOffset,
                    (tabOrigin.Y - window.Position.Y) / window.RenderScaling + panel.Bounds.Height / 2);
                _current = surface;
            }
            else
            {
                // A custom template may finish layout on the next frame.
                _current = new TabDragBehavior(window.TabHost);
                _windowGrab = new Vector(_grabOffset + 6, 22);
            }
            _floatingWindow = window;
            _enteredThroughWindow = false;
            _touchedHosts.Add(window.TabHost);
            source.UpdateDragState(OvoTabDragState.None);
            MoveFloatingWindow(point);
            SetState(OvoTabDragState.Detach);
            return true;
        }
        finally
        {
            _transitioning = false;
        }
    }

    private void CommitVisualOrder()
    {
        if (_dragPanel is not { } panel) return;
        var index = panel.DropIndex;
        panel.EndDrag();
        _dragPanel = null;
        if (_current.Host.Tabs.Contains(_tab))
            _current.Host.MoveTab(_tab, Math.Clamp(index, 0, _current.Host.Tabs.Count - 1));
    }

    private void MoveFloatingWindow(PixelPoint point)
    {
        if (_floatingWindow is not { WindowState: WindowState.Normal } window) return;
        window.Position = new PixelPoint(point.X - (int)Math.Round(_windowGrab.X * window.RenderScaling),
            point.Y - (int)Math.Round(_windowGrab.Y * window.RenderScaling));
    }

    private void OnScrollTick(object? sender, EventArgs e)
    {
        if (_floatingWindow is not null || !_dragging || _finished) return;
        if (_current.ScrollViewer is not { } scroll ||
            !_current.Contains(_lastPoint, OvoTabDragBoundary.TabStrip)) return;
        var x = scroll.PointToClient(_lastPoint).X;
        const double edge = 32;
        var speed = x < edge ? -Math.Clamp((edge - x) / edge, 0, 1) * 14
            : x > scroll.Bounds.Width - edge ? Math.Clamp((x - scroll.Bounds.Width + edge) / edge, 0, 1) * 14 : 0;
        if (Math.Abs(speed) < double.Epsilon) return;
        scroll.Offset = new Vector(Math.Clamp(scroll.Offset.X + speed, 0,
            Math.Max(0, scroll.Extent.Width - scroll.Viewport.Width)), scroll.Offset.Y);
        scroll.UpdateLayout();
        DragInStrip(_lastPoint);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!ReferenceEquals(e.Pointer, _pointer) || e.InitialPressMouseButton != MouseButton.Left) return;
        if (_dragging) ContinueDrag(ScreenPoint(e));
        Finish();
        e.Handled = true;
    }

    private void OnCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (ReferenceEquals(e.Pointer, _pointer) && ReferenceEquals(e.Source, _captureRoot)) Finish();
    }

    private void OnCaptureWindowClosed(object? sender, EventArgs e) => Finish();

    internal void Finish()
    {
        if (_finished) return;
        _finished = true;
        _scrollTimer.Stop();
        _scrollTimer.Tick -= OnScrollTick;
        _captureRoot.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        _captureRoot.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        _captureRoot.RemoveHandler(InputElement.PointerCaptureLostEvent, OnCaptureLost);
        _captureRoot.Closed -= OnCaptureWindowClosed;
        if (ReferenceEquals(_pointer.Captured, _captureRoot)) _pointer.Capture(null);
        CommitVisualOrder();
        _tab.IsDragging = false;
        foreach (var host in _touchedHosts) host.UpdateDragState(OvoTabDragState.None);
        if (ReferenceEquals(_service.ActiveDrag, this)) _service.ActiveDrag = null;
        foreach (var (window, settings) in _windowSettings)
        {
            window.Opacity = settings.Opacity;
            window.ShowActivated = settings.ShowActivated;
            window.Topmost = settings.Topmost;
        }
        if (_createdWindow is { } created && created.TabHost.Tabs.Count == 0) created.CloseAfterTransfer();
        foreach (var host in _touchedHosts)
        {
            if (!ReferenceEquals(host, _createdWindow?.TabHost)) _service.CloseEmptyWindowIfNeeded(host);
        }
        if (_dragging) (TopLevel.GetTopLevel(_current.Host) as Window)?.Activate();
    }

    private void RememberWindow(Window window) =>
        _windowSettings.TryAdd(window, (window.Opacity, window.ShowActivated, window.Topmost));

    private void SetState(OvoTabDragState state)
    {
        _current.Host.UpdateDragState(state);
        if (!ReferenceEquals(Origin.Host, _current.Host)) Origin.Host.UpdateDragState(state);
    }

    private PixelPoint ScreenPoint(PointerEventArgs e) => _captureRoot.PointToScreen(e.GetPosition(_captureRoot));
}

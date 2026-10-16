using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace OvoUi.Tab.Internal;

internal sealed class TabDragBehavior(OvoTabHost host)
{
    private const double DragThreshold = 5;
    private Control? _container;
    private Border? _draggedElement;
    private OvoTabItem? _draggedTab;
    private Point _dragStart;
    private PixelPoint _lastScreenPoint;
    private bool _dragging;
    private bool _movingWindow;
    private OvoTabHost? _targetHost;
    private OvoTabDragPreviewWindow? _preview;

    public OvoTabDragState State { get; private set; }

    public void Attach(Control container)
    {
        if (ReferenceEquals(_container, container)) return;
        Detach();
        _container = container;
        container.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, handledEventsToo: true);
        container.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, handledEventsToo: true);
        container.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, handledEventsToo: true);
        container.AddHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost, handledEventsToo: true);
    }

    public void Detach()
    {
        if (_container is not null)
        {
            _container.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
            _container.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
            _container.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
            _container.RemoveHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost);
        }
        EndDrag();
        _container = null;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_container is null || e.Pointer.Type != PointerType.Mouse ||
            !e.GetCurrentPoint(_container).Properties.IsLeftButtonPressed || IsInteractiveElement(e.Source as Visual))
            return;
        if (OvoTabHost.FindTabRoot(e.Source as Visual) is not { Tag: OvoTabItem tab } tabRoot) return;

        host.SelectedTab = tab;

        // On macOS the window-owned strip remains title-bar chrome; tab rearranging and detaching stay disabled.
        if (OperatingSystem.IsMacOS() && TopLevel.GetTopLevel(host) is OvoTabWindow macWindow &&
            ReferenceEquals(macWindow.TabHost, host) &&
            macWindow.IsTabStripWindowDragEnabled &&
            macWindow.WindowState != WindowState.FullScreen)
        {
            macWindow.BeginMoveDrag(e);
            e.Handled = true;
            return;
        }

        if (host.EffectiveDragMode == OvoTabDragMode.Disabled) return;
        _draggedElement = tabRoot;
        _draggedTab = tab;
        _dragStart = e.GetPosition(_container);
        _lastScreenPoint = GetScreenPosition(e);
        _dragging = false;
        _movingWindow = false;
        SetState(OvoTabDragState.None);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_container is null || _draggedTab is null) return;
        var point = e.GetPosition(_container);
        var delta = point - _dragStart;
        if (!_dragging && Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y) <= DragThreshold) return;

        var screenPoint = GetScreenPosition(e);
        if (!_dragging)
        {
            _dragging = true;
            _movingWindow = host.Tabs.Count == 1 && host.TabService.Hosts.Count == 1 &&
                            TopLevel.GetTopLevel(host) is OvoTabWindow { IsTabStripWindowDragEnabled: true } window &&
                            ReferenceEquals(window.TabHost, host);
            _draggedTab.IsDragging = !_movingWindow;
            if (!_movingWindow && _draggedElement is not null) _draggedElement.Opacity = 0.35;
            if (_movingWindow)
                SetState(OvoTabDragState.MoveWindow);
            else
                ShowPreview(screenPoint);
            e.Pointer.Capture(_container);
        }

        if (_movingWindow)
        {
            MoveWindow(screenPoint);
            return;
        }

        UpdateState(point, screenPoint);
        _preview?.MoveTo(screenPoint);
        if (State == OvoTabDragState.Reorder) ReorderAt(point);
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragging && _draggedTab is not null)
        {
            var screenPoint = GetScreenPosition(e);
            if (State == OvoTabDragState.Transfer && _targetHost is not null)
                host.TabService.Transfer(_draggedTab, host, _targetHost);
            else if (State == OvoTabDragState.Detach)
                host.TabService.Detach(_draggedTab, host, screenPoint);
        }

        e.Pointer.Capture(null);
        EndDrag();
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) => EndDrag();

    private void UpdateState(Point containerPoint, PixelPoint screenPoint)
    {
        if (_container is null) return;
        if (new Rect(_container.Bounds.Size).Contains(containerPoint))
        {
            _targetHost = null;
            SetState(OvoTabDragState.Reorder);
            return;
        }

        if (host.EffectiveDragMode == OvoTabDragMode.ReorderOnly)
        {
            _targetHost = null;
            SetState(OvoTabDragState.Blocked);
            return;
        }

        _targetHost = host.TabService.FindHostAt(screenPoint, host);
        if (_targetHost is not null)
        {
            SetState(OvoTabDragState.Transfer);
            return;
        }

        SetState(host.EffectiveDragMode == OvoTabDragMode.ReorderAndDetach && host.TabService.WindowFactory is not null
            ? OvoTabDragState.Detach
            : OvoTabDragState.Blocked);
    }

    private void ReorderAt(Point point)
    {
        if (_container is null || _draggedTab is null) return;
        var target = _container.GetVisualDescendants()
            .OfType<Border>()
            .Where(border => border.Classes.Contains("tab-root") && border.Tag is OvoTabItem)
            .FirstOrDefault(border =>
            {
                var translated = border.TranslatePoint(default, _container);
                return translated.HasValue && new Rect(translated.Value, border.Bounds.Size).Contains(point);
            });
        if (target?.Tag is not OvoTabItem targetTab || ReferenceEquals(targetTab, _draggedTab)) return;
        host.MoveTab(_draggedTab, host.Tabs.IndexOf(targetTab));
    }

    private void MoveWindow(PixelPoint screenPoint)
    {
        if (TopLevel.GetTopLevel(host) is not Window { WindowState: WindowState.Normal } window) return;
        var delta = screenPoint - _lastScreenPoint;
        window.Position = new PixelPoint(window.Position.X + delta.X, window.Position.Y + delta.Y);
        _lastScreenPoint = screenPoint;
    }

    private void ShowPreview(PixelPoint screenPoint)
    {
        if (_draggedTab is null || OperatingSystem.IsBrowser() || OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
            return;
        _preview = new OvoTabDragPreviewWindow();
        _preview.UpdateTab(_draggedTab);
        _preview.MoveTo(screenPoint);
        _preview.Show();
    }

    private void SetState(OvoTabDragState state)
    {
        if (State == state) return;
        State = state;
        host.UpdateDragState(state);
        _preview?.UpdateState(state);
    }

    private void EndDrag()
    {
        if (_draggedTab is not null) _draggedTab.IsDragging = false;
        if (_draggedElement is not null) _draggedElement.Opacity = 1;
        _preview?.Close();
        _preview = null;
        _draggedTab = null;
        _draggedElement = null;
        _targetHost = null;
        _dragging = false;
        _movingWindow = false;
        SetState(OvoTabDragState.None);
    }

    private PixelPoint GetScreenPosition(PointerEventArgs e)
    {
        if (TopLevel.GetTopLevel(host) is not Visual root) return default;
        return root.PointToScreen(e.GetPosition(root));
    }

    private static bool IsInteractiveElement(Visual? visual)
    {
        while (visual is not null)
        {
            if (visual is Button) return true;
            if (visual is Border border && border.Classes.Contains("tab-root")) return false;
            visual = visual.GetVisualParent();
        }
        return false;
    }
}

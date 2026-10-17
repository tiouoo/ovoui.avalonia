using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using OvoUi.Controls;

namespace OvoUi.Tab.Internal;

internal sealed class TabDragBehavior(OvoTabHost host)
{
    internal OvoTabHost Host => host;
    internal SelectionList? Container { get; private set; }
    internal OvoTabPanel? Panel => Container?.ItemsPanelRoot as OvoTabPanel;
    internal ScrollViewer? ScrollViewer => Container?.FindAncestorOfType<ScrollViewer>();

    internal Control? Area => Container?.GetVisualAncestors().OfType<Control>()
        .FirstOrDefault(control => control.Name == "PART_TabStripArea") ?? Container;

    public void Attach(SelectionList container)
    {
        if (ReferenceEquals(Container, container)) return;
        Detach();
        Container = container;
        host.DragSurfaces.Add(this);
        container.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, handledEventsToo: true);
    }

    public void Detach()
    {
        if (Container is not null)
            Container.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        if (ReferenceEquals(host.TabService.ActiveDrag?.Origin, this))
            host.TabService.ActiveDrag.Finish();
        host.DragSurfaces.Remove(this);
        Container = null;
    }

    internal bool Contains(PixelPoint screenPoint, OvoTabDragBoundary boundary, double tolerance = 0)
    {
        if (Area is not { } area || !area.IsVisible ||
            area.GetVisualAncestors().Any(visual => !visual.IsVisible) ||
            TopLevel.GetTopLevel(area) is not Window { IsVisible: true, WindowState: not WindowState.Minimized } window)
            return false;

        var region = boundary == OvoTabDragBoundary.Window ? (Control)window : area;
        var point = region.PointToClient(screenPoint);
        var size = region is Window ? window.ClientSize : region.Bounds.Size;
        return new Rect(size).Inflate(tolerance).Contains(point);
    }

    internal Point PanelPoint(PixelPoint screenPoint) => Panel?.PointToClient(screenPoint) ?? default;

    internal int InsertionIndex(PixelPoint screenPoint)
    {
        if (Panel is not { } panel) return host.Tabs.Count;
        var x = panel.PointToClient(screenPoint).X;
        return panel.Children.Count(child => x > child.Bounds.X + child.Bounds.Width / 2);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Container is null || e.Pointer.Type != PointerType.Mouse ||
            !e.GetCurrentPoint(Container).Properties.IsLeftButtonPressed || IsInteractiveElement(e.Source as Visual))
            return;
        if (OvoTabHost.FindTabRoot(e.Source as Visual) is not { Tag: OvoTabItem tab }) return;

        host.SetCurrentValue(OvoTabHost.SelectedTabProperty, tab);
        if (OperatingSystem.IsMacOS() && TopLevel.GetTopLevel(host) is OvoTabWindow macWindow &&
            ReferenceEquals(macWindow.TabHost, host) && macWindow.IsTabStripWindowDragEnabled &&
            macWindow.WindowState != WindowState.FullScreen)
        {
            macWindow.BeginMoveDrag(e);
            e.Handled = true;
            return;
        }

        if (host.EffectiveDragMode == OvoTabDragMode.Disabled ||
            TopLevel.GetTopLevel(Container) is not Window window) return;
        host.TabService.ActiveDrag?.Finish();
        host.TabService.ActiveDrag = new TabDragSession(this, tab, window, e);
        e.Handled = true;
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
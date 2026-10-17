using Avalonia;
using Avalonia.Controls;
using OvoUi.Tab.Internal;

namespace OvoUi.Tab;

/// <summary>
/// Coordinates transfers between a related set of tab hosts.
/// </summary>
public sealed class OvoTabService
{
    private readonly List<WeakReference<OvoTabHost>> _hosts = [];
    internal TabDragSession? ActiveDrag { get; set; }

    public OvoTabService()
    {
        WindowFactory = context => new OvoTabWindow(context.Service);
    }

    public static OvoTabService Shared { get; } = new();

    /// <summary>
    /// Creates a window when a tab is detached. Set to null to disable detaching for this service.
    /// </summary>
    public Func<OvoTabWindowCreationContext, OvoTabWindow?>? WindowFactory { get; set; }

    public bool CloseEmptySecondaryWindows { get; set; } = true;

    public IReadOnlyList<OvoTabHost> Hosts
    {
        get
        {
            Compact();
            return _hosts.Select(reference => reference.TryGetTarget(out var host) ? host : null)
                .Where(host => host is not null)
                .Cast<OvoTabHost>()
                .ToArray();
        }
    }

    internal void Register(OvoTabHost host)
    {
        Compact();
        if (_hosts.Any(reference => reference.TryGetTarget(out var existing) && ReferenceEquals(existing, host)))
            return;
        _hosts.Add(new WeakReference<OvoTabHost>(host));
    }

    internal void Unregister(OvoTabHost host)
    {
        _hosts.RemoveAll(reference => !reference.TryGetTarget(out var existing) || ReferenceEquals(existing, host));
    }

    public bool Transfer(OvoTabItem tab, OvoTabHost source, OvoTabHost target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return Transfer(tab, source, target, target.Tabs.Count, closeEmpty: true, activate: true);
    }

    internal bool Transfer(OvoTabItem tab, OvoTabHost source, OvoTabHost target, int index,
        bool closeEmpty, bool activate)
    {
        ArgumentNullException.ThrowIfNull(tab);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        if (ReferenceEquals(source, target) || !source.Tabs.Contains(tab) || target.Tabs.Contains(tab)) return false;

        VisualDetach.FromParent(tab.Content);
        source.RemoveForTransfer(tab);
        target.InsertTransferredTab(tab, index);
        if (closeEmpty) CloseEmptyWindowIfNeeded(source);
        if (activate) (TopLevel.GetTopLevel(target) as Window)?.Activate();
        return true;
    }

    public OvoTabWindow? Detach(OvoTabItem tab, OvoTabHost source, PixelPoint screenPosition)
    {
        ArgumentNullException.ThrowIfNull(tab);
        ArgumentNullException.ThrowIfNull(source);
        if (!source.Tabs.Contains(tab) || WindowFactory is null) return null;

        var window = CreateDetachedWindow(tab, source, screenPosition);
        if (window is null) return null;

        VisualDetach.FromParent(tab.Content);
        source.RemoveForTransfer(tab);
        window.TabHost.AddTab(tab, select: true);
        window.Position = new PixelPoint(screenPosition.X - 52, screenPosition.Y - 22);
        window.Show();
        CloseEmptyWindowIfNeeded(source);
        return window;
    }

    internal OvoTabWindow? CreateDetachedWindow(OvoTabItem tab, OvoTabHost source, PixelPoint screenPosition)
    {
        var window = WindowFactory?.Invoke(new OvoTabWindowCreationContext(this, source, tab, screenPosition));
        if (window is null) return null;
        window.TabService = this;
        var target = window.TabHost;
        target.DragMode = source.DragMode;
        target.DetachBoundary = source.DetachBoundary;
        target.AttachBoundary = source.AttachBoundary;
        target.DetachDistance = source.DetachDistance;
        target.IsTabAttachEnabled = source.IsTabAttachEnabled;
        target.Strings = source.Strings;
        target.PageTransition = source.PageTransition;
        target.NewTabFactory ??= source.NewTabFactory;
        target.CloseGuard ??= source.CloseGuard;
        return window;
    }

    internal TabDragBehavior? FindSurfaceAt(PixelPoint screenPoint, OvoTabHost excluded,
        Window? excludedWindow = null)
        => Hosts.OrderBy(host => (TopLevel.GetTopLevel(host) as Window)?.IsActive == true).Reverse()
            .Where(host => !ReferenceEquals(host, excluded) && host.IsTabAttachEnabled &&
                           host.EffectiveDragMode == OvoTabDragMode.ReorderAndDetach &&
                           !ReferenceEquals(TopLevel.GetTopLevel(host), excludedWindow))
            .SelectMany(host => host.DragSurfaces)
            .FirstOrDefault(surface => surface.Contains(screenPoint, surface.Host.AttachBoundary));

    internal void CloseEmptyWindowIfNeeded(OvoTabHost host)
    {
        if (!CloseEmptySecondaryWindows || host.Tabs.Count != 0) return;
        if (TopLevel.GetTopLevel(host) is OvoTabWindow { IsPrimaryWindow: false } window)
            window.CloseAfterTransfer();
    }

    private void Compact() => _hosts.RemoveAll(reference => !reference.TryGetTarget(out _));
}

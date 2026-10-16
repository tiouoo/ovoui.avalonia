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
        ArgumentNullException.ThrowIfNull(tab);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        if (ReferenceEquals(source, target) || !source.Tabs.Contains(tab)) return false;

        VisualDetach.FromParent(tab.Content);
        source.RemoveForTransfer(tab);
        target.AddTab(tab, select: true);
        CloseEmptyWindowIfNeeded(source);
        (TopLevel.GetTopLevel(target) as Window)?.Activate();
        return true;
    }

    public OvoTabWindow? Detach(OvoTabItem tab, OvoTabHost source, PixelPoint screenPosition)
    {
        ArgumentNullException.ThrowIfNull(tab);
        ArgumentNullException.ThrowIfNull(source);
        if (!source.Tabs.Contains(tab) || WindowFactory is null) return null;

        var window = WindowFactory(new OvoTabWindowCreationContext(this, source, tab, screenPosition));
        if (window is null) return null;

        window.TabService = this;
        VisualDetach.FromParent(tab.Content);
        source.RemoveForTransfer(tab);
        window.TabHost.AddTab(tab, select: true);
        window.Position = new PixelPoint(screenPosition.X - 52, screenPosition.Y - 22);
        window.Show();
        CloseEmptyWindowIfNeeded(source);
        return window;
    }

    internal OvoTabHost? FindHostAt(PixelPoint screenPoint, OvoTabHost? excluded = null)
    {
        foreach (var host in Hosts.Reverse())
        {
            if (ReferenceEquals(host, excluded) || !host.IsVisible || TopLevel.GetTopLevel(host) is not Window window ||
                !window.IsVisible)
                continue;

            var origin = host.PointToScreen(default);
            var scaling = window.RenderScaling;
            var width = host.Bounds.Width * scaling;
            var height = host.Bounds.Height * scaling;
            if (screenPoint.X >= origin.X && screenPoint.X <= origin.X + width &&
                screenPoint.Y >= origin.Y && screenPoint.Y <= origin.Y + height)
                return host;
        }

        return null;
    }

    private void CloseEmptyWindowIfNeeded(OvoTabHost host)
    {
        if (!CloseEmptySecondaryWindows || host.Tabs.Count != 0) return;
        if (TopLevel.GetTopLevel(host) is OvoTabWindow { IsPrimaryWindow: false } window)
            window.CloseAfterTransfer();
    }

    private void Compact() => _hosts.RemoveAll(reference => !reference.TryGetTarget(out _));
}

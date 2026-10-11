using System.Collections.Concurrent;
using System.Diagnostics;

namespace OvoUi.Controls;

internal record struct HostKey(string? Id, int? Hash);

internal static class OverlayDialogManager
{
    private static readonly ConcurrentDictionary<HostKey, OverlayDialogHost> Hosts = new();

    public static void RegisterHost(OverlayDialogHost host, string? id, int? hash)
    {
        Debug.WriteLine("Count: " + Hosts.Count);
        Hosts.TryAdd(new HostKey(id, hash), host);
    }

    public static void UnregisterHost(string? id, int? hash)
    {
        Hosts.TryRemove(new HostKey(id, hash), out _);
    }

    public static OverlayDialogHost? GetHost(string? id, int? hash)
    {
        var match = Hosts.FirstOrDefault(pair =>
            (hash is null || pair.Key.Hash == hash) &&
            pair.Key.Id == id);
        if (match.Value is null)
        {
            Debug.Assert(false,
                "OverlayDialogHost not found. Please ensure the host is properly set up with one of below method:\n" +
                "1. Use OvoWindow or OvoView, which include a default OverlayDialogHost, or\n" +
                "2. Add an OverlayDialogHost control with a HostId property to your view.");
            return null;
        }

        return match.Value;
    }
}

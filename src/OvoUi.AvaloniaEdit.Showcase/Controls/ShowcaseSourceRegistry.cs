using System.Collections.Concurrent;
using System.ComponentModel;

namespace OvoUi.AvaloniaEdit.Showcase.Controls;

/// <summary>
/// Stores XAML snippets emitted by the ControlShowcase source generator.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ShowcaseSourceRegistry
{
    private static readonly ConcurrentDictionary<string, string> Sources =
        new(StringComparer.Ordinal);

    public static void Register(string sourceKey, string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        Sources[sourceKey] = source;
    }

    public static bool TryGet(string? sourceKey, out string source)
    {
        if (!string.IsNullOrWhiteSpace(sourceKey) && Sources.TryGetValue(sourceKey, out var value))
        {
            source = value;
            return true;
        }

        source = string.Empty;
        return false;
    }
}

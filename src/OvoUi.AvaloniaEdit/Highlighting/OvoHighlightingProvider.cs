using System.Xml;
using Avalonia.Platform;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

namespace OvoUi.AvaloniaEdit.Highlighting;

internal static class OvoHighlightingProvider
{
    private static readonly Lazy<IReadOnlyDictionary<string, IHighlightingDefinition>> Definitions =
        new(LoadDefinitions);

    public static IHighlightingDefinition? Find(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return null;

        return Definitions.Value.TryGetValue(language.Trim(), out var definition) ? definition : null;
    }

    private static IReadOnlyDictionary<string, IHighlightingDefinition> LoadDefinitions()
    {
        var definitions = new Dictionary<string, IHighlightingDefinition>(StringComparer.OrdinalIgnoreCase);
        Load("Config", ["config", "configuration"], definitions);
        Load("MinecraftLog", ["minecraftlog", "minecraft-log", "mclog"], definitions);
        return definitions;
    }

    private static void Load(
        string name,
        IEnumerable<string> aliases,
        IDictionary<string, IHighlightingDefinition> definitions)
    {
        var uri = new Uri($"avares://OvoUi.AvaloniaEdit/Assets/Highlighting/{name}.xshd");
        using var stream = AssetLoader.Open(uri);
        using var reader = XmlReader.Create(stream);
        var definition = HighlightingLoader.Load(reader, HighlightingManager.Instance);

        definitions[name] = definition;
        foreach (var alias in aliases)
            definitions[alias] = definition;
    }
}

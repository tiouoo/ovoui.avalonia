using System.Xml;
using System.Xml.Linq;
using Avalonia.Platform;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

namespace OvoUi.AvaloniaEdit.Highlighting;

internal static class OvoHighlightingProvider
{
    private static readonly IReadOnlyDictionary<string, string> LightPalette =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Comment"] = "#A0A1A7",
            ["Key"] = "#E45649",
            ["String"] = "#C44A00",
            ["Quote"] = "#E00070",
            ["Number"] = "#986801",
            ["Literal"] = "#A626A4",
            ["Section"] = "#4078F2",
            ["Punctuation"] = "#4078F2",
            ["Delimiter"] = "#E45649",
            ["Tag"] = "#E45649",
            ["PropertyElement"] = "#006FE6",
            ["Attribute"] = "#006FE6",
            ["NamespacePrefix"] = "#0184BC",
            ["NamespaceDeclaration"] = "#006FE6",
            ["NamespaceIdentifier"] = "#A626A4",
            ["MarkupExtension"] = "#A626A4",
            ["MarkupParameter"] = "#9D9D9F",
            ["Special"] = "#986801",
            ["Entity"] = "#0184BC",
            ["CData"] = "#4078F2",
            ["DocType"] = "#4078F2",
            ["Command"] = "#986801",
            ["Argument"] = "#383A42",
            ["Option"] = "#9D9D9F",
            ["Variable"] = "#0184BC",
            ["Operator"] = "#9D9D9F",
            ["Keyword"] = "#A626A4",
            ["Information"] = "#4078F2",
            ["Warning"] = "#986801",
            ["Error"] = "#E45649",
            ["Debug"] = "#A626A4",
            ["Trace"] = "#9D9D9F",
            ["Logger"] = "#50A14F"
        };

    private static readonly IReadOnlyDictionary<string, string> DarkPalette =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Comment"] = "#7F848E",
            ["Key"] = "#D56A72",
            ["String"] = "#FFD166",
            ["Quote"] = "#FF4D9D",
            ["Number"] = "#C7A06C",
            ["Literal"] = "#BF8FFC",
            ["Section"] = "#7885E0",
            ["Punctuation"] = "#7885E0",
            ["Delimiter"] = "#BF8FFC",
            ["Tag"] = "#BF8FFC",
            ["PropertyElement"] = "#4FC3FF",
            ["Attribute"] = "#4FC3FF",
            ["NamespacePrefix"] = "#7885E0",
            ["NamespaceDeclaration"] = "#4FC3FF",
            ["NamespaceIdentifier"] = "#BF8FFC",
            ["MarkupExtension"] = "#BF8FFC",
            ["MarkupParameter"] = "#787878",
            ["Special"] = "#C7A06C",
            ["Entity"] = "#4FC3FF",
            ["CData"] = "#7885E0",
            ["DocType"] = "#7885E0",
            ["Command"] = "#F9F1A5",
            ["Argument"] = "#CCCCCC",
            ["Option"] = "#838383",
            ["Variable"] = "#45B1B4",
            ["Operator"] = "#A5A5A5",
            ["Keyword"] = "#BF8FFC",
            ["Information"] = "#7885E0",
            ["Warning"] = "#C7A06C",
            ["Error"] = "#D56A72",
            ["Debug"] = "#BF8FFC",
            ["Trace"] = "#787878",
            ["Logger"] = "#7D9462"
        };

    private static readonly Lazy<IReadOnlyDictionary<string, IHighlightingDefinition>> LightDefinitions =
        new(() => LoadDefinitions(LightPalette));

    private static readonly Lazy<IReadOnlyDictionary<string, IHighlightingDefinition>> DarkDefinitions =
        new(() => LoadDefinitions(DarkPalette));

    public static IHighlightingDefinition? Find(string language, bool useDarkPalette)
    {
        if (string.IsNullOrWhiteSpace(language))
            return null;

        var definitions = useDarkPalette ? DarkDefinitions.Value : LightDefinitions.Value;
        var key = language.Trim().TrimStart('.');
        return definitions.TryGetValue(key, out var definition) ? definition : null;
    }

    private static IReadOnlyDictionary<string, IHighlightingDefinition> LoadDefinitions(
        IReadOnlyDictionary<string, string> palette)
    {
        var definitions = new Dictionary<string, IHighlightingDefinition>(StringComparer.OrdinalIgnoreCase);
        Load(
            "Axaml",
            ["axaml", "xaml", "xml"],
            palette,
            definitions);
        Load(
            "Config",
            ["config", "configuration", "json", "json5", "toml", "yaml", "yml", "properties", "cfg", "conf", "ini"],
            palette,
            definitions);
        Load(
            "Shell",
            ["bash", "shell", "shellscript", "sh", "zsh"],
            palette,
            definitions);
        Load(
            "MinecraftLog",
            ["minecraftlog", "minecraft-log", "mclog"],
            palette,
            definitions);
        return definitions;
    }

    private static void Load(
        string name,
        IEnumerable<string> aliases,
        IReadOnlyDictionary<string, string> palette,
        IDictionary<string, IHighlightingDefinition> definitions)
    {
        var uri = new Uri($"avares://OvoUi.AvaloniaEdit/Assets/Highlighting/{name}.xshd");
        using var stream = AssetLoader.Open(uri);
        var document = XDocument.Load(stream);

        foreach (var color in document.Root?.Elements().Where(element => element.Name.LocalName == "Color") ?? [])
        {
            var nameAttribute = color.Attribute("name");
            if (nameAttribute is not null && palette.TryGetValue(nameAttribute.Value, out var foreground))
                color.SetAttributeValue("foreground", foreground);
        }

        using var reader = document.CreateReader();
        var definition = HighlightingLoader.Load(reader, HighlightingManager.Instance);

        definitions[name] = definition;
        foreach (var alias in aliases)
            definitions[alias] = definition;
    }
}

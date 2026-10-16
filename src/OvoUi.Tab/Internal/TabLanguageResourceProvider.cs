using OvoUi.Common.Language;
using OvoUi.Common.Language.Langs;

namespace OvoUi.Tab.Internal;

internal sealed class TabLanguageResourceProvider : ILanguageResourceProvider
{
    public static TabLanguageResourceProvider Instance { get; } = new();

    private static readonly IReadOnlyDictionary<string, object?> English =
        new Dictionary<string, object?> { ["OvoTab.Strings"] = OvoTabStrings.English };

    private static readonly IReadOnlyDictionary<string, object?> Chinese =
        new Dictionary<string, object?> { ["OvoTab.Strings"] = OvoTabStrings.SimplifiedChinese };

    public IReadOnlyDictionary<string, object?> GetResources(ILang language) =>
        language is LangEnUs ? English : Chinese;
}

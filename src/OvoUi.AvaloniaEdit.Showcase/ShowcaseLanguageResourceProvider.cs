using OvoUi.Common.Language;
using OvoUi.Common.Language.Langs;

namespace OvoUi.AvaloniaEdit.Showcase;

internal sealed class ShowcaseLanguageResourceProvider : ILanguageResourceProvider
{
    public static ShowcaseLanguageResourceProvider Instance { get; } = new();

    private static readonly IReadOnlyDictionary<string, object?> Chinese =
        new Dictionary<string, object?>
        {
            ["Showcase.Preview"] = "预览",
            ["Showcase.Code"] = "代码",
            ["Showcase.Combined"] = "分屏"
        };

    private static readonly IReadOnlyDictionary<string, object?> English =
        new Dictionary<string, object?>
        {
            ["Showcase.Preview"] = "Preview",
            ["Showcase.Code"] = "Code",
            ["Showcase.Combined"] = "Split"
        };

    public IReadOnlyDictionary<string, object?> GetResources(ILang language) =>
        language is LangEnUs ? English : Chinese;
}

namespace OvoUi.Common.Language;

/// <summary>
/// Lets optional packages contribute resources that follow the active OvoUi language.
/// </summary>
public interface ILanguageResourceProvider
{
    IReadOnlyDictionary<string, object?> GetResources(ILang language);
}

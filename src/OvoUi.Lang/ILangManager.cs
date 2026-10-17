using System.Globalization;

namespace OvoUi.Lang;

public interface ILangManager
{
    CultureInfo CurrentCulture { get; }

    IObservable<CultureInfo> CultureChanges { get; }

    void UpdateCulture(CultureInfo culture);

    IObservable<string?>? GetObservable(string key);

    void AddResources(CultureInfo culture, IReadOnlyDictionary<string, string> resources);

    IReadOnlyDictionary<CultureInfo, string> GetTranslations(LangObservableString observable);

    void ClearRuntimeResources();
}

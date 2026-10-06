using System.ComponentModel;
using System.Reflection;
using Avalonia;
using OvoUi.Common.Language.Langs;

namespace OvoUi.Common.Language;

public class LangManager : INotifyPropertyChanged
{
    private readonly List<ILanguageResourceProvider> _resourceProviders = [];
    private ILang _current = new LangZhCn();

    private LangManager()
    {
        UpdateResources();
    }

    public static LangManager Instance { get; } = new();

    public ILang Current
    {
        get => _current;
        set
        {
            if (_current == value) return;
            _current = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Current)));
            UpdateResources();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public static void SetLanguage(Languages lang)
    {
        Instance.Current = lang switch
        {
            Languages.zh_cn => new LangZhCn(),
            Languages.en_us => new LangEnUs(),
            _ => new LangZhCn()
        };
    }

    public static void SetLanguage(ILang customLang)
    {
        Instance.Current = customLang;
    }

    public static void RegisterResourceProvider(ILanguageResourceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (Instance._resourceProviders.Contains(provider))
            return;

        Instance._resourceProviders.Add(provider);
        UpdateResources(provider.GetResources(Instance.Current));
    }

    private void UpdateResources()
    {
        if (Application.Current == null) return;

        var properties = typeof(ILang).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        UpdateResources(properties.ToDictionary(
            property => $"Lang.{property.Name}",
            property => (object?)(property.GetValue(_current)?.ToString() ?? string.Empty)));

        foreach (var provider in _resourceProviders)
            UpdateResources(provider.GetResources(_current));
    }

    private static void UpdateResources(IReadOnlyDictionary<string, object?> resources)
    {
        if (Application.Current == null)
            return;

        foreach (var (key, value) in resources)
        {
            // ReSharper disable once RedundantDictionaryContainsKeyBeforeAdding
            if (Application.Current.Resources.ContainsKey(key))
            {
                Application.Current.Resources[key] = value;
            }
            else
            {
                Application.Current.Resources.Add(key, value);
            }
        }
    }
}

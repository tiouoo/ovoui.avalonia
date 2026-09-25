using System.ComponentModel;
using System.Reflection;
using Avalonia;
using OvoUi.Common.Language.Langs;

namespace OvoUi.Common.Language;

public class LangManager : INotifyPropertyChanged
{
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

    private void UpdateResources()
    {
        if (Application.Current == null) return;

        var properties = typeof(ILang).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var key = $"Lang.{prop.Name}";
            var value = prop.GetValue(_current)?.ToString() ?? string.Empty;

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
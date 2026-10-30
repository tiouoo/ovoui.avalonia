using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        UpdateResources(new Dictionary<string, object?>
        {
            ["Lang.Name"] = _current.Name,
            ["Lang.FileName"] = _current.FileName,
            ["Lang.UpdateAt"] = _current.UpdateAt,
            ["Lang.Type"] = _current.Type,
            ["Lang.Size"] = _current.Size,
            ["Lang.Cancel"] = _current.Cancel,
            ["Lang.Close"] = _current.Close,
            ["Lang.Confirm"] = _current.Confirm,
            ["Lang.Ok"] = _current.Ok,
            ["Lang.NavSwitch"] = _current.NavSwitch,
            ["Lang.ShowHiddenFiles"] = _current.ShowHiddenFiles,
            ["Lang.FileAlreadyExists"] = _current.FileAlreadyExists,
            ["Lang.Day"] = _current.Day,
            ["Lang.Month"] = _current.Month,
            ["Lang.Year"] = _current.Year,
            ["Lang.Hour"] = _current.Hour,
            ["Lang.Minute"] = _current.Minute,
            ["Lang.Second"] = _current.Second,
            ["Lang.Undo"] = _current.Undo,
            ["Lang.Redo"] = _current.Redo,
            ["Lang.SelectAll"] = _current.SelectAll,
            ["Lang.Cut"] = _current.Cut,
            ["Lang.Copy"] = _current.Copy,
            ["Lang.Copied"] = _current.Copied,
            ["Lang.WordWrap"] = _current.WordWrap,
            ["Lang.Paste"] = _current.Paste,
            ["Lang.Clear"] = _current.Clear,
            ["Lang.JumpTo"] = _current.JumpTo,
            ["Lang.Page"] = _current.Page,
            ["Lang.StartTime"] = _current.StartTime,
            ["Lang.EndTime"] = _current.EndTime,
            ["Lang.Yes"] = _current.Yes,
            ["Lang.No"] = _current.No,
            ["Lang.BringForward"] = _current.BringForward,
            ["Lang.BringToFront"] = _current.BringToFront,
            ["Lang.SendBackward"] = _current.SendBackward,
            ["Lang.SendToBack"] = _current.SendToBack,
            ["Lang.Maximum"] = _current.Maximum,
            ["Lang.Minimum"] = _current.Minimum,
            ["Lang.Restore"] = _current.Restore,
            ["Lang.On"] = _current.On,
            ["Lang.Off"] = _current.Off
        });

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

using System;
using System.Collections.Generic;
using System.Globalization;

namespace OvoUi.Lang;

public abstract class LangManagerBase : ILangManager
{
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _resources;
    private readonly Dictionary<string, Dictionary<string, string>> _overrides = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, LangObservableString> _observables = new(StringComparer.Ordinal);
    private readonly LangObservable<CultureInfo> _cultures;
    private readonly object _gate = new();
    private readonly Queue<CultureInfo> _pending = new();
    private bool _updating;

    protected LangManagerBase(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> resources,
        string defaultCulture = "")
    {
        _resources = resources;
        CurrentCulture = CultureInfo.GetCultureInfo(defaultCulture);
        _cultures = new LangObservable<CultureInfo>("", CurrentCulture);
        foreach (var pair in resources[string.Empty])
            _observables.Add(pair.Key, new LangObservableString(pair.Key, Resolve(CurrentCulture, pair.Key)));
    }

    public CultureInfo CurrentCulture { get; private set; }

    public IObservable<CultureInfo> CultureChanges => _cultures;

    protected LangObservableString GetString(string key) => _observables[key];

    public IObservable<string?>? GetObservable(string key) => _observables.GetValueOrDefault(key);

    public void UpdateCulture(CultureInfo culture)
    {
        lock (_gate)
        {
            _pending.Enqueue(culture ?? CultureInfo.InvariantCulture);
            if (_updating) return;
            _updating = true;
            try
            {
                while (_pending.Count > 0)
                {
                    CurrentCulture = _pending.Dequeue();
                    foreach (var pair in _observables)
                        pair.Value.OnNext(Resolve(CurrentCulture, pair.Key));
                    _cultures.OnNext(CurrentCulture);
                }
            }
            finally
            {
                _updating = false;
                _pending.Clear();
            }
        }
    }

    private string? Resolve(CultureInfo culture, string key)
    {
        for (var current = culture;; current = current.Parent)
        {
            if (_overrides.TryGetValue(current.Name, out var runtime) && runtime.TryGetValue(key, out var custom))
                return custom;
            if (_resources.TryGetValue(current.Name, out var compiled) && compiled.TryGetValue(key, out var value))
                return value;
            if (current.Name.Length == 0) return null;
        }
    }

    public void AddResources(CultureInfo culture, IReadOnlyDictionary<string, string> resources)
    {
        ArgumentNullException.ThrowIfNull(culture);
        ArgumentNullException.ThrowIfNull(resources);
        lock (_gate)
        {
            if (!_overrides.TryGetValue(culture.Name, out var values))
                _overrides[culture.Name] = values = new(StringComparer.Ordinal);
            foreach (var pair in resources) values[pair.Key] = pair.Value;
        }
    }

    public IReadOnlyDictionary<CultureInfo, string> GetTranslations(LangObservableString observable)
    {
        ArgumentNullException.ThrowIfNull(observable);
        lock (_gate)
        {
            var result = new Dictionary<CultureInfo, string>();
            foreach (var pair in _resources)
                if (pair.Value.TryGetValue(observable.Key, out var value))
                    result[CultureInfo.GetCultureInfo(pair.Key)] = value;
            foreach (var pair in _overrides)
                if (pair.Value.TryGetValue(observable.Key, out var value))
                    result[CultureInfo.GetCultureInfo(pair.Key)] = value;
            return result;
        }
    }

    public void ClearRuntimeResources()
    {
        lock (_gate) _overrides.Clear();
    }
}
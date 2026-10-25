using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OvoUi.Lang;

public sealed class LangFormatBuilder
{
    private readonly ILangManager _manager;
    private readonly IObservable<string?> _format;
    private readonly List<object?> _slots = [];
    private readonly List<(Func<IObserver<object?>, IDisposable> Subscribe, int Index)> _sources = [];
    private readonly List<(PropertyChange Change, int Index)> _properties = [];
    private readonly List<(Func<object?> Read, int Index)> _computed = [];

    private bool _built;

    internal LangFormatBuilder(
        ILangManager manager,
        IObservable<string?> format)
    {
        _manager = manager;
        _format = format;
    }

    public LangFormatBuilder Arg(object? value)
    {
        ThrowIfBuilt();
        _slots.Add(value);
        return this;
    }

    public LangFormatBuilder Arg<T>(IObservable<T> observable)
    {
        ArgumentNullException.ThrowIfNull(observable);
        ThrowIfBuilt();

        _slots.Add(null);
        _sources.Add((observer => observable.Subscribe(new BoxingObserver<T>(observer)), _slots.Count - 1));
        return this;
    }

    public LangFormatBuilder Arg<T>(Func<T> getter)
    {
        ArgumentNullException.ThrowIfNull(getter);
        ThrowIfBuilt();

        _slots.Add(null);
        _computed.Add((() => getter(), _slots.Count - 1));
        return this;
    }

    public LangFormatBuilder Arg<TSource>(
        TSource source,
        string propertyName,
        Func<TSource, object?> getter)
        where TSource : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ArgumentNullException.ThrowIfNull(getter);
        ThrowIfBuilt();

        var change = new PropertyChange(source, propertyName, () => getter(source));
        _slots.Add(null);
        _properties.Add((change, _slots.Count - 1));
        return this;
    }

    public LangFormatObservable Build()
    {
        _built = true;
        return new LangFormatObservable(
            _manager,
            _format,
            [.. _slots],
            [.. _sources],
            [.. _properties],
            [.. _computed]);
    }

    private void ThrowIfBuilt()
    {
        if (_built)
            throw new InvalidOperationException(
                "The format builder has already been built and can no longer be modified.");
    }
}
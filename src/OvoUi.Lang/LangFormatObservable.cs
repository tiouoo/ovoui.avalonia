using System.ComponentModel;
using System.Globalization;

namespace OvoUi.Lang;

public sealed class LangFormatObservable : IObservable<string?>
{
    private readonly ILangManager _manager;
    private readonly IObservable<string?> _format;
    private readonly object?[] _currentArgs;
    private readonly (Func<IObserver<object?>, IDisposable> Subscribe, int Index)[] _sourceArgs;
    private readonly (PropertyChange Change, int Index)[] _propertyArgs;
    private readonly (Func<object?> Read, int Index)[] _computedArgs;

    private readonly object _gate = new();
    private readonly Queue<(FormatSubscription[] Observers, string Value)> _notifications = new();

    private FormatSubscription[] _observers = [];
    private bool _attached;
    private bool _attaching;
    private bool _notifying;
    private string? _currentFormat;
    private string? _lastEmitted;
    private IDisposable? _formatSubscription;
    private IDisposable? _cultureSubscription;
    private IDisposable[] _argSubscriptions = [];
    private PropertyChangedEventHandler? _propertyHandler;

    internal LangFormatObservable(
        ILangManager manager,
        IObservable<string?> format,
        object?[] initialArgs,
        (Func<IObserver<object?>, IDisposable> Subscribe, int Index)[] sources,
        (PropertyChange Change, int Index)[] properties,
        (Func<object?> Read, int Index)[] computedArgs)
    {
        _manager = manager;
        _format = format;
        _currentArgs = initialArgs;
        _sourceArgs = sources;
        _propertyArgs = properties;
        _computedArgs = computedArgs;
    }

    public IDisposable Subscribe(IObserver<string?> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        var subscription = new FormatSubscription(this, observer);

        lock (_gate)
        {
            _observers = [.. _observers, subscription];
            if (!_attached)
            {
                _attached = true;
                _attaching = true;
                try
                {
                    Attach();
                }
                catch
                {
                    DetachSources();
                    _attached = false;
                    _observers = RemoveOne(_observers, subscription);
                    throw;
                }
                finally
                {
                    _attaching = false;
                }
            }

            _lastEmitted = ComputeCore();
            try
            {
                NotifyCore([subscription], _lastEmitted);
            }
            catch
            {
                subscription.Dispose();
                throw;
            }
        }

        return subscription;
    }

    private void Attach()
    {
        _formatSubscription = _format.Subscribe(new FormatObserver(this));

        _argSubscriptions = new IDisposable[_sourceArgs.Length];
        for (var i = 0; i < _sourceArgs.Length; i++)
        {
            _argSubscriptions[i] = _sourceArgs[i].Subscribe(new ArgObserver(this, _sourceArgs[i].Index));
        }

        _cultureSubscription = _manager.CultureChanges.Subscribe(new CultureObserver(this));

        if (_propertyArgs.Length > 0 || _computedArgs.Length > 0)
        {
            if (_propertyArgs.Length > 0)
            {
                _propertyHandler = OnPropertyChanged;
                foreach (var source in _propertyArgs.Select(p => p.Change.Source)
                             .Distinct<INotifyPropertyChanged>(ReferenceEqualityComparer.Instance))
                    source.PropertyChanged += _propertyHandler;
            }

            foreach (var (change, index) in _propertyArgs)
                _currentArgs[index] = change.Getter();
            foreach (var (read, index) in _computedArgs)
                _currentArgs[index] = read();
        }
    }

    private void OnFormatValue(string? value)
    {
        lock (_gate)
        {
            _currentFormat = value;
            EmitIfChangedCore();
        }
    }

    private void OnArgValue(int index, object? value)
    {
        lock (_gate)
        {
            _currentArgs[index] = value;
            EmitIfChangedCore();
        }
    }

    private void OnCultureChanged()
    {
        lock (_gate)
        {
            EmitIfChangedCore();
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var wildcard = string.IsNullOrEmpty(e.PropertyName);

        lock (_gate)
        {
            foreach (var (change, index) in _propertyArgs)
            {
                if (!ReferenceEquals(sender, change.Source) ||
                    (!wildcard && e.PropertyName != change.PropertyName))
                    continue;

                _currentArgs[index] = change.Getter();
            }

            EmitIfChangedCore();
        }
    }

    private void EmitIfChangedCore()
    {
        if (_attaching || _observers.Length == 0)
            return;

        var result = ComputeCore();
        if (result == _lastEmitted)
            return;

        _lastEmitted = result;
        NotifyCore(_observers, result);
    }

    private void NotifyCore(FormatSubscription[] observers, string value)
    {
        if (observers.Length == 0)
            return;

        _notifications.Enqueue((observers, value));
        if (_notifying)
            return;

        _notifying = true;
        try
        {
            while (_notifications.TryDequeue(out var notification))
            {
                foreach (var observer in notification.Observers)
                    observer.OnNext(notification.Value);
            }
        }
        finally
        {
            _notifying = false;
            _notifications.Clear();
        }
    }

    public void Refresh()
    {
        lock (_gate)
        {
            foreach (var (change, index) in _propertyArgs)
                _currentArgs[index] = change.Getter();
            foreach (var (read, index) in _computedArgs)
                _currentArgs[index] = read();

            var result = ComputeCore();
            _lastEmitted = result;
            NotifyCore(_observers, result);
        }
    }

    private string ComputeCore()
    {
        var format = _currentFormat;
        if (string.IsNullOrEmpty(format))
            return string.Empty;

        try
        {
            return string.Format(_manager.CurrentCulture, format, _currentArgs);
        }
        catch (FormatException)
        {
            return format;
        }
    }

    private void Unsubscribe(FormatSubscription observer)
    {
        lock (_gate)
        {
            _observers = RemoveOne(_observers, observer);
            if (_observers.Length > 0 || !_attached)
                return;

            _attached = false;
            DetachSources();
        }
    }

    private void DetachSources()
    {
        _formatSubscription?.Dispose();
        _formatSubscription = null;
        _cultureSubscription?.Dispose();
        _cultureSubscription = null;
        foreach (var subscription in _argSubscriptions)
            subscription?.Dispose();
        _argSubscriptions = [];

        if (_propertyHandler is not null)
        {
            foreach (var source in _propertyArgs.Select(p => p.Change.Source)
                         .Distinct<INotifyPropertyChanged>(ReferenceEqualityComparer.Instance))
                source.PropertyChanged -= _propertyHandler;
            _propertyHandler = null;
        }
    }

    private static FormatSubscription[] RemoveOne(FormatSubscription[] observers, FormatSubscription observer)
    {
        var index = Array.FindIndex(observers, o => ReferenceEquals(o, observer));
        if (index < 0)
            return observers;

        var result = new FormatSubscription[observers.Length - 1];
        Array.Copy(observers, result, index);
        Array.Copy(observers, index + 1, result, index, observers.Length - index - 1);
        return result;
    }

    private sealed class FormatSubscription(LangFormatObservable parent, IObserver<string?> observer) : IDisposable
    {
        private int _disposed;

        public void OnNext(string value)
        {
            if (Volatile.Read(ref _disposed) == 0)
                observer.OnNext(value);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                parent.Unsubscribe(this);
        }
    }

    private sealed class FormatObserver(LangFormatObservable parent) : IObserver<string?>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(string? value) => parent.OnFormatValue(value);
    }

    private sealed class ArgObserver(LangFormatObservable parent, int argIndex) : IObserver<object?>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(object? value) => parent.OnArgValue(argIndex, value);
    }

    private sealed class CultureObserver(LangFormatObservable parent) : IObserver<CultureInfo>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(CultureInfo value) => parent.OnCultureChanged();
    }
}
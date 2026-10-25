using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OvoUi.Lang;

public class LangObservable<T> : IObservable<T>
{
    private readonly object _gate = new();
    private readonly Queue<(Subscription[] Targets, T Value)> _queue = new();
    private Subscription[] _subscriptions = [];
    private T _currentValue;
    private bool _notifying;

    public string Key { get; }

    public T CurrentValue
    {
        get
        {
            lock (_gate) return _currentValue;
        }
    }

    public LangObservable(string key, T initialValue)
    {
        ArgumentNullException.ThrowIfNull(key);
        Key = key;
        _currentValue = initialValue;
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        var subscription = new Subscription(this, observer);
        lock (_gate)
        {
            _subscriptions = [.. _subscriptions, subscription];
            _queue.Enqueue(([subscription], _currentValue));
        }

        try
        {
            Drain();
        }
        catch
        {
            subscription.Dispose();
            throw;
        }

        return subscription;
    }

    public void OnNext(T value)
    {
        lock (_gate)
        {
            _currentValue = value;
            _queue.Enqueue((_subscriptions, value));
        }

        Drain();
    }

    private void Drain()
    {
        lock (_gate)
        {
            if (_notifying) return;
            _notifying = true;
        }

        try
        {
            while (true)
            {
                (Subscription[] Targets, T Value) item;
                lock (_gate)
                {
                    if (_queue.Count == 0)
                    {
                        _notifying = false;
                        return;
                    }

                    item = _queue.Dequeue();
                }

                foreach (var target in item.Targets) target.Notify(item.Value);
            }
        }
        catch
        {
            lock (_gate)
            {
                _notifying = false;
                _queue.Clear();
            }

            throw;
        }
    }

    public void ClearSubscribers()
    {
        lock (_gate)
        {
            foreach (var subscription in _subscriptions) subscription.Deactivate();
            _subscriptions = [];
        }
    }

    private sealed class Subscription(LangObservable<T> owner, IObserver<T> observer) : IDisposable
    {
        private int _disposed;
        public void Deactivate() => Interlocked.Exchange(ref _disposed, 1);

        public void Notify(T value)
        {
            if (Volatile.Read(ref _disposed) == 0) observer.OnNext(value);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            lock (owner._gate)
                owner._subscriptions = owner._subscriptions.Where(s => !ReferenceEquals(s, this)).ToArray();
        }
    }
}
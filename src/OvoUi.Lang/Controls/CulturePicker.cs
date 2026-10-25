using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using OvoUi.Lang;

namespace OvoUi.Lang;

public class CulturePicker : TemplatedControl
{
    public static readonly StyledProperty<IList<ILangManager>?> ManagersProperty =
        AvaloniaProperty.Register<CulturePicker, IList<ILangManager>?>(nameof(Managers));

    public static readonly StyledProperty<IList<LangCulture>?> CulturesProperty =
        AvaloniaProperty.Register<CulturePicker, IList<LangCulture>?>(nameof(Cultures));

    public static readonly StyledProperty<LangCulture?> SelectedItemProperty =
        AvaloniaProperty.Register<CulturePicker, LangCulture?>(nameof(SelectedItem));

    public static readonly RoutedEvent<CultureChangedEventArgs> CultureChangedEvent =
        RoutedEvent.Register<CulturePicker, CultureChangedEventArgs>(
            nameof(CultureChanged), RoutingStrategies.Bubble);

    public IList<ILangManager>? Managers
    {
        get => GetValue(ManagersProperty);
        set => SetValue(ManagersProperty, value);
    }

    public IList<LangCulture>? Cultures
    {
        get => GetValue(CulturesProperty);
        set => SetValue(CulturesProperty, value);
    }

    public LangCulture? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public event EventHandler<CultureChangedEventArgs> CultureChanged
    {
        add => AddHandler(CultureChangedEvent, value);
        remove => RemoveHandler(CultureChangedEvent, value);
    }

    private IDisposable? _primaryCultureSubscription;
    private INotifyCollectionChanged? _observedManagers;
    private INotifyCollectionChanged? _observedCultures;
    private bool _suppressSync;
    private bool _suspended;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _suspended = false;
        WireCollectionChanged(Managers, ref _observedManagers, OnManagersCollectionChanged);
        WireCollectionChanged(Cultures, ref _observedCultures, OnCulturesCollectionChanged);
        OnManagersChanged();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _suspended = true;
        _primaryCultureSubscription?.Dispose();
        _primaryCultureSubscription = null;
        WireCollectionChanged<ILangManager>(null, ref _observedManagers, OnManagersCollectionChanged);
        WireCollectionChanged<LangCulture>(null, ref _observedCultures, OnCulturesCollectionChanged);
    }

    public CulturePicker()
    {
        var defaultManagers = new ObservableCollection<ILangManager>();
        var defaultCultures = new ObservableCollection<LangCulture>();

        SetCurrentValue(ManagersProperty, defaultManagers);
        SetCurrentValue(CulturesProperty, defaultCultures);

        WireCollectionChanged(defaultManagers, ref _observedManagers, OnManagersCollectionChanged);
        WireCollectionChanged(defaultCultures, ref _observedCultures, OnCulturesCollectionChanged);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (_suspended) return;

        if (change.Property == SelectedItemProperty)
        {
            OnSelectedItemChanged(change.GetNewValue<LangCulture?>());
        }
        else if (change.Property == ManagersProperty)
        {
            var newList = change.GetNewValue<IList<ILangManager>?>();
            if (!ReferenceEquals(change.GetOldValue<IList<ILangManager>?>(), newList))
                WireCollectionChanged(newList, ref _observedManagers, OnManagersCollectionChanged);
            OnManagersChanged();
        }
        else if (change.Property == CulturesProperty)
        {
            var newList = change.GetNewValue<IList<LangCulture>?>();
            if (!ReferenceEquals(change.GetOldValue<IList<LangCulture>?>(), newList))
                WireCollectionChanged(newList, ref _observedCultures, OnCulturesCollectionChanged);
            SyncSelectedItemFromPrimary();
        }
    }

    private void OnSelectedItemChanged(LangCulture? item)
    {
        if (_suppressSync || item is null)
            return;

        var managers = Managers;
        if (managers is null) return;

        _suppressSync = true;
        try
        {
            foreach (var manager in managers)
            {
                manager.UpdateCulture(item.Culture);
            }
        }
        finally
        {
            _suppressSync = false;
        }

        RaiseEvent(new CultureChangedEventArgs(CultureChangedEvent, item));
    }

    private void OnManagersChanged()
    {
        var managers = Managers;

        _primaryCultureSubscription?.Dispose();
        _primaryCultureSubscription = null;

        var primary = managers is not null && managers.Count > 0 ? managers[0] : null;
        if (primary is not null)
        {
            _primaryCultureSubscription = primary.CultureChanges.Subscribe(
                new CultureChangeObserver(this));
        }

        SyncSelectedItemFromPrimary();
    }

    private void OnManagersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnManagersChanged();
    }

    private void OnCulturesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SyncSelectedItemFromPrimary();
    }

    private void SyncSelectedItemFromPrimary()
    {
        var managers = Managers;
        var cultures = Cultures;

        var primary = managers is not null && managers.Count > 0 ? managers[0] : null;
        if (primary is null || cultures is null || cultures.Count == 0)
        {
            SetCurrentValue(SelectedItemProperty, null);
            return;
        }

        var current = primary.CurrentCulture;
        foreach (var item in cultures)
        {
            if (!Equals(item.Culture, current)) continue;
            SetCurrentValue(SelectedItemProperty, item);
            return;
        }

        SetCurrentValue(SelectedItemProperty, null);
    }

    private static void WireCollectionChanged<T>(
        IList<T>? newList,
        ref INotifyCollectionChanged? field,
        NotifyCollectionChangedEventHandler handler)
    {
        if (ReferenceEquals(field, newList))
            return;

        if (field is not null)
            field.CollectionChanged -= handler;

        field = newList as INotifyCollectionChanged;

        if (field is not null)
            field.CollectionChanged += handler;
    }

    private sealed class CultureChangeObserver(CulturePicker owner) : IObserver<CultureInfo>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(CultureInfo value)
        {
            if (owner._suppressSync || owner._suspended) return;
            if (Dispatcher.UIThread.CheckAccess()) owner.SyncSelectedItemFromPrimary();
            else
                Dispatcher.UIThread.Post(() =>
                {
                    if (!owner._suspended) owner.SyncSelectedItemFromPrimary();
                });
        }
    }
}
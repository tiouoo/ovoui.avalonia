using Avalonia.Interactivity;

namespace OvoUi.Controls;

public class ValueChangedEventArgs<T> : RoutedEventArgs where T : struct, IComparable<T>
{
    public ValueChangedEventArgs(RoutedEvent routedEvent, T? oldValue, T? newValue) : base(routedEvent)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public T? OldValue { get; }
    public T? NewValue { get; }
}
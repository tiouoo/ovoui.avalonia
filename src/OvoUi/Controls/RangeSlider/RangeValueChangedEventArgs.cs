using Avalonia.Interactivity;

namespace OvoUi.Controls;

public class RangeValueChangedEventArgs : RoutedEventArgs
{
    public RangeValueChangedEventArgs(
        RoutedEvent routedEvent,
        object source,
        double oldValue,
        double newValue,
        bool isLower = true) : base(routedEvent, source)
    {
        OldValue = oldValue;
        NewValue = newValue;
        IsLower = isLower;
    }

    public RangeValueChangedEventArgs(
        RoutedEvent routedEvent,
        double oldValue,
        double newValue,
        bool isLower = true) : base(routedEvent)
    {
        OldValue = oldValue;
        NewValue = newValue;
        IsLower = isLower;
    }

    public double OldValue { get; set; }
    public double NewValue { get; set; }
    public bool IsLower { get; set; }
}
using Avalonia.Interactivity;

namespace OvoUi.Common.Classes;

public class ResultEventArgs : RoutedEventArgs
{
    public ResultEventArgs(object? result)
    {
        Result = result;
    }

    public ResultEventArgs(RoutedEvent routedEvent, object? result) : base(routedEvent)
    {
        Result = result;
    }

    public object? Result { get; set; }
}
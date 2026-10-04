using Avalonia.Interactivity;

namespace OvoUi.Controls;

public class PinCodeCompleteEventArgs(IList<string> code, RoutedEvent? @event) : RoutedEventArgs(@event)
{
    public IList<string> Code { get; } = code;
}
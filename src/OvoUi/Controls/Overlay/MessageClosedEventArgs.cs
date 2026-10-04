using Avalonia.Interactivity;

namespace OvoUi.Controls;

public class MessageClosedEventArgs(MessageCloseReason reason) : RoutedEventArgs
{
    public MessageCloseReason Reason { get; } = reason;
}

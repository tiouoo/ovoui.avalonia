using Avalonia.Interactivity;
using OvoUi.Common.Classes;

namespace OvoUi.Controls;

public class DialogLayerChangeEventArgs : RoutedEventArgs
{
    public DialogLayerChangeEventArgs(DialogLayerChangeType type)
    {
        ChangeType = type;
    }

    public DialogLayerChangeEventArgs(RoutedEvent routedEvent, DialogLayerChangeType type) : base(routedEvent)
    {
        ChangeType = type;
    }

    public DialogLayerChangeType ChangeType { get; }
}
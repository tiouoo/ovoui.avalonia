using Avalonia.Interactivity;

namespace OvoUi.Common.Classes;

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

public enum DialogLayerChangeType
{
    BringForward,
    SendBackward,
    BringToFront,
    SendToBack
}
using Avalonia.Interactivity;

namespace OvoUi.Lang;

public class CultureChangedEventArgs : RoutedEventArgs
{
    public LangCulture? Culture { get; }

    public CultureChangedEventArgs(RoutedEvent routedEvent, LangCulture? culture)
        : base(routedEvent)
    {
        Culture = culture;
    }
}

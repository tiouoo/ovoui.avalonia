using System.ComponentModel;

namespace OvoUi.Tab;

public sealed class OvoTabClosingEventArgs(OvoTabItem tab, OvoTabCloseReason reason) : CancelEventArgs
{
    public OvoTabItem Tab { get; } = tab;
    public OvoTabCloseReason Reason { get; } = reason;
}

public sealed class OvoTabEventArgs(OvoTabItem tab) : EventArgs
{
    public OvoTabItem Tab { get; } = tab;
}

public sealed class OvoTabDragStateChangedEventArgs(OvoTabDragState oldState, OvoTabDragState newState) : EventArgs
{
    public OvoTabDragState OldState { get; } = oldState;
    public OvoTabDragState NewState { get; } = newState;
}

public sealed class OvoTabMenuOpeningEventArgs(OvoTabItem tab, ICollection<Avalonia.Controls.MenuItem> items)
    : EventArgs
{
    public OvoTabItem Tab { get; } = tab;
    public ICollection<Avalonia.Controls.MenuItem> Items { get; } = items;
}

public sealed class OvoTabNewRequestedEventArgs : EventArgs
{
    public OvoTabItem? Tab { get; set; }
}
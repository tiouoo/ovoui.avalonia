using System.Collections.ObjectModel;
using Avalonia.Controls.Notifications;

namespace OvoUi.Common.Classes;

public class NotificationEntry
{
    public NotificationEntry(Notification Entry,
        NotificationType Type,
        DateTime Time,
        ObservableCollection<OperateButtonEntry>? OperateButtons = null)
    {
        this.Entry = Entry;
        this.Type = Type;
        this.Time = Time;
        this.OperateButtons = OperateButtons;
    }

    public ObservableCollection<OperateButtonEntry>? OperateButtons { get; set; }
    public NotificationEntry Self => this;
    public Notification Entry { get; init; }
    public NotificationType Type { get; init; }
    public DateTime Time { get; init; }
}
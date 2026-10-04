using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Notifications;
using Avalonia.Metadata;
using INotification = OvoUi.Common.Interfaces.INotification;

namespace OvoUi.Controls;

public class Notification : INotification, INotifyPropertyChanged
{
    public Notification(
        string? title,
        string? content,
        NotificationType type = NotificationType.Information,
        TimeSpan? expiration = null,
        bool showClose = true,
        Action? onClick = null,
        Action<Controls.MessageCloseReason>? onClose = null)
    {
        Title = title;
        Content = content;
        Type = type;
        Expiration = expiration ?? TimeSpan.FromSeconds(3);
        ShowClose = showClose;
        OnClick = onClick;
        OnClose = onClose;
    }

    public Notification() : this(null, null)
    {
    }

    public string? Title
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    [Content]
    public string? Content
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public NotificationType Type { get; set; }

    public TimeSpan Expiration { get; set; }

    public bool ShowIcon { get; set; }

    public bool ShowClose { get; }

    public Action? OnClick { get; set; }

    public Action<Controls.MessageCloseReason>? OnClose { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
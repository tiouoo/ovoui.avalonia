using System.Collections.ObjectModel;
using Avalonia.Controls.Notifications;
using OvoUi.Common.Classes;
using OvoUi.Controls;

namespace OvoUi.Common.Classes;

public class NotificationOptions
{
    public NotificationOptions(Action<MessageCloseReason>? onClose = null)
    {
        OnClose = onClose;
    }

    public string? Title { get; set; }
    public object? Content { get; set; }
    public NotificationType Type { get; set; } = NotificationType.Information;
    public TimeSpan Expiration { get; set; } = TimeSpan.FromSeconds(3);
    public Action? OnClick { get; set; }
    public Action<MessageCloseReason>? OnClose { get; set; }
    public Action? OnRemove { get; set; }
    public ObservableCollection<OperateButtonEntry>? OperateButtons { get; set; }
    public bool IsButtonsInline { get; set; } = true;
    public bool IsTouchClose { get; set; } = false;
    public bool IsIconVisible { get; set; } = true;
    public bool IsCloseButtonVisible { get; set; } = true;
    public bool IsCollapseButtonVisible { get; set; } = false;
    public bool IsColorful { get; set; } = true;
    public Avalonia.Controls.Classes Classes { get; } = ["Light"];
}
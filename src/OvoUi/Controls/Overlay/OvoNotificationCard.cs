using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Notifications;
using Avalonia.LogicalTree;

namespace OvoUi.Controls;

/// <summary>
/// Control that represents and displays a notification.
/// </summary>
[PseudoClasses(
    OvoNotificationManager.PC_TopLeft,
    OvoNotificationManager.PC_TopRight,
    OvoNotificationManager.PC_BottomLeft,
    OvoNotificationManager.PC_BottomRight,
    OvoNotificationManager.PC_TopCenter,
    OvoNotificationManager.PC_BottomCenter
)]
public class OvoNotificationCard : OvoMessageCard
{
    public static readonly DirectProperty<OvoNotificationCard, NotificationPosition> PositionProperty =
        AvaloniaProperty.RegisterDirect<OvoNotificationCard, NotificationPosition>(nameof(Position),
            o => o.Position, (o, v) => o.Position = v);

    public NotificationPosition Position
    {
        get;
        set => SetAndRaise(PositionProperty, ref field, value);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        UpdatePseudoClasses(Position);
    }

    private void UpdatePseudoClasses(NotificationPosition position)
    {
        PseudoClasses.Set(OvoNotificationManager.PC_TopLeft, position == NotificationPosition.TopLeft);
        PseudoClasses.Set(OvoNotificationManager.PC_TopRight, position == NotificationPosition.TopRight);
        PseudoClasses.Set(OvoNotificationManager.PC_BottomLeft, position == NotificationPosition.BottomLeft);
        PseudoClasses.Set(OvoNotificationManager.PC_BottomRight, position == NotificationPosition.BottomRight);
        PseudoClasses.Set(OvoNotificationManager.PC_TopCenter, position == NotificationPosition.TopCenter);
        PseudoClasses.Set(OvoNotificationManager.PC_BottomCenter, position == NotificationPosition.BottomCenter);
    }
}
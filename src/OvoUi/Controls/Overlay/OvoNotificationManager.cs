using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OvoUi.Common.Classes;
using OvoUi.Common.Classes;
using INotification = OvoUi.Common.Interfaces.INotification;
using INotificationManager = OvoUi.Common.Interfaces.INotificationManager;

namespace OvoUi.Controls;

[PseudoClasses(PC_TopLeft, PC_TopRight, PC_BottomLeft, PC_BottomRight, PC_TopCenter, PC_BottomCenter)]
public class OvoNotificationManager : WindowMessageManager, INotificationManager
{
    public const string PC_TopLeft = ":topleft";
    public const string PC_TopRight = ":topright";
    public const string PC_BottomLeft = ":bottomleft";
    public const string PC_BottomRight = ":bottomright";
    public const string PC_TopCenter = ":topcenter";
    public const string PC_BottomCenter = ":bottomcenter";

    public static readonly StyledProperty<NotificationPosition> PositionProperty =
        AvaloniaProperty.Register<OvoNotificationManager, NotificationPosition>(nameof(Position),
            NotificationPosition.TopRight);

    static OvoNotificationManager()
    {
        HorizontalAlignmentProperty.OverrideDefaultValue<OvoNotificationManager>(HorizontalAlignment.Stretch);
        VerticalAlignmentProperty.OverrideDefaultValue<OvoNotificationManager>(VerticalAlignment.Stretch);
    }

    public OvoNotificationManager()
    {
        UpdatePseudoClasses(Position);
    }

    public OvoNotificationManager(TopLevel? host) : this()
    {
        if (host is not null)
        {
            InstallFromTopLevel(host);
        }
    }

    public OvoNotificationManager(VisualLayerManager? visualLayerManager) : base(visualLayerManager)
    {
        UpdatePseudoClasses(Position);
    }

    public NotificationPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public void Show(INotification content)
    {
        var options = new NotificationOptions
        {
            Type = content.Type,
            Expiration = content.Expiration,
            IsIconVisible = content.ShowIcon,
            IsCloseButtonVisible = content.ShowClose,
            OnClick = content.OnClick,
            OnClose = content.OnClose,
            Title = content.Title,
            Content = content.Content
        };
        Show(options);
    }

    public static bool TryGetNotificationManager(Visual? visual, out OvoNotificationManager? manager)
    {
        manager = visual?.FindDescendantOfType<OvoNotificationManager>();
        return manager is not null;
    }

    public override void Show(object content)
    {
        if (content is INotification notification)
        {
            Show(notification);
        }
        else
        {
            Show(new NotificationOptions()
            {
                Content = content
            });
        }
    }

    public void Show(string title, string msg, NotificationOptions? options = null)
    {
        options ??= new NotificationOptions();
        options.Title = title;
        options.Content = msg;
        Show(options);
    }

    public void Show(string msg, NotificationOptions? options = null)
    {
        options ??= new NotificationOptions();
        options.Content = msg;
        Show(options);
    }

    public async void Show(NotificationOptions options)
    {
        Dispatcher.UIThread.VerifyAccess();

        var notificationControl = new OvoNotificationCard
        {
            Content = options is { Title: not null, Content: string msg }
                ? new Notification
                {
                    Content = msg,
                    Title = options.Title
                }
                : options.Content ?? options.Title,
            NotificationType = options.Type,
            ShowIcon = options.IsIconVisible,
            OperateButtons = options.OperateButtons,
            IsButtonsInline = options.IsButtonsInline,
            OnRemove = options.OnRemove,
            ShowCollapseButton = options.IsCollapseButtonVisible,
            ShowRemoveButton = options.IsCloseButtonVisible,
            [!OvoNotificationCard.PositionProperty] = this[!PositionProperty]
        };

        if (options.IsColorful)
            options.Classes.Add("Colorful");

        if (options.Classes is not null)
        {
            foreach (var @class in options.Classes)
            {
                notificationControl.Classes.Add(@class);
            }
        }

        notificationControl.MessageClosed += (sender, args) =>
        {
            options.OnClose?.Invoke(args.Reason);
            _items?.Remove(sender);
        };


        notificationControl.PointerPressed += (_, _) =>
        {
            if (options.IsTouchClose)
                notificationControl.Close();
            options.OnClick?.Invoke();
        };

        Dispatcher.UIThread.Post(() =>
        {
            _items?.Add(notificationControl);

            if (_items?.OfType<OvoNotificationCard>().Count(i => !i.IsClosing) > MaxItems)
            {
                _items.OfType<OvoNotificationCard>().First(i => !i.IsClosing).Close(MessageCloseReason.Displaced);
            }
        });

        if (options.Expiration == TimeSpan.Zero)
        {
            return;
        }

        await Task.Delay(options.Expiration);

        notificationControl.Close(MessageCloseReason.Timeout);
    }

    /// <inheritdoc/>
    public void Close(INotification notification)
    {
        Dispatcher.UIThread.VerifyAccess();

        _items?.OfType<OvoNotificationCard>()
            .FirstOrDefault(i => i.Content == notification)
            ?.Close(MessageCloseReason.UserAction);
    }

    /// <inheritdoc/>
    public void CloseAll()
    {
        Dispatcher.UIThread.VerifyAccess();

        var items = _items?.OfType<OvoNotificationCard>().ToList();
        if (items is null) return;
        foreach (var item in items)
        {
            item.Close(MessageCloseReason.UserAction);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PositionProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<NotificationPosition>());
        }
    }

    private void UpdatePseudoClasses(NotificationPosition position)
    {
        PseudoClasses.Set(PC_TopLeft, position == NotificationPosition.TopLeft);
        PseudoClasses.Set(PC_TopRight, position == NotificationPosition.TopRight);
        PseudoClasses.Set(PC_BottomLeft, position == NotificationPosition.BottomLeft);
        PseudoClasses.Set(PC_BottomRight, position == NotificationPosition.BottomRight);
        PseudoClasses.Set(PC_TopCenter, position == NotificationPosition.TopCenter);
        PseudoClasses.Set(PC_BottomCenter, position == NotificationPosition.BottomCenter);
    }
}
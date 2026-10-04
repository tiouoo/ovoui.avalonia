using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OvoUi.Common.Classes;
using OvoUi.Common.Interfaces;
using OvoUi.Common.Classes;

namespace OvoUi.Controls;

public class OvoToastManager : WindowMessageManager, IToastManager
{
    public OvoToastManager()
    {
    }

    public OvoToastManager(TopLevel? host) : this()
    {
        if (host is not null) InstallFromTopLevel(host);
    }

    public OvoToastManager(VisualLayerManager? visualLayerManager) : base(visualLayerManager)
    {
    }

    public void Show(IToast content)
    {
        var options = new NotificationOptions
        {
            Content = content.Content,
            Type = content.Type,
            Expiration = content.Expiration,
            IsIconVisible = content.ShowIcon,
            IsCloseButtonVisible = content.ShowClose,
            OnClick = content.OnClick,
            OnClose = content.OnClose
        };
        Show(options);
    }

    public void Show(string msg, NotificationType type = NotificationType.Information)
    {
        var toastOptions = new NotificationOptions
        {
            Content = msg,
            Type = type,
        };

        Show(toastOptions);
    }

    public void Show(string msg, NotificationOptions options)
    {
        options.Content = msg;
        Show(options);
    }

    public static bool TryGetToastManager(Visual? visual, out OvoToastManager? manager)
    {
        manager = visual?.FindDescendantOfType<OvoToastManager>();
        return manager is not null;
    }

    public override void Show(object content)
    {
        if (content is IToast toast)
            Show(toast);
        else
            Show(new NotificationOptions()
            {
                Content = content
            });
    }

    public async void Show(NotificationOptions options)
    {
        Dispatcher.UIThread.VerifyAccess();

        var toastControl = new OvoToastCard
        {
            Content = options.Content,
            NotificationType = options.Type,
            ShowIcon = options.IsIconVisible,
            OperateButtons = options.OperateButtons,
            IsButtonsInline = options.IsButtonsInline,
            OnRemove = options.OnRemove,
            ShowCollapseButton = options.IsCollapseButtonVisible,
            ShowRemoveButton = options.IsCloseButtonVisible
        };

        if (options.IsColorful)
            options.Classes.Add("Colorful");

        if (options.Classes is not null)
            foreach (var @class in options.Classes)
                toastControl.Classes.Add(@class);

        toastControl.MessageClosed += (sender, args) =>
        {
            options.OnClose?.Invoke(args.Reason);
            _items?.Remove(sender);
        };

        toastControl.PointerPressed += (_, _) =>
        {
            if (options.IsTouchClose)
                toastControl.Close();
            options.OnClick?.Invoke();
        };

        Dispatcher.UIThread.Post(() =>
        {
            _items?.Add(toastControl);

            if (_items?.OfType<OvoToastCard>().Count(i => !i.IsClosing) > MaxItems)
                _items.OfType<OvoToastCard>().First(i => !i.IsClosing).Close(MessageCloseReason.Displaced);
        });

        if (options.Expiration == TimeSpan.Zero) return;

        await Task.Delay(options.Expiration);

        toastControl.CloseWithoutRemovingFromList(MessageCloseReason.Timeout);
    }

    /// <inheritdoc/>
    public void Close(IToast toast)
    {
        Dispatcher.UIThread.VerifyAccess();

        _items?.OfType<OvoToastCard>()
            .FirstOrDefault(i => i.Content == toast)
            ?.Close(MessageCloseReason.UserAction);
    }

    /// <inheritdoc/>
    public void CloseAll()
    {
        Dispatcher.UIThread.VerifyAccess();

        var items = _items?.OfType<OvoToastCard>().ToList();
        if (items is null) return;
        foreach (var item in items)
        {
            item.Close(MessageCloseReason.UserAction);
        }
    }
}
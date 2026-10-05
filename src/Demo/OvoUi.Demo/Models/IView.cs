using OvoUi.Controls;

namespace OvoUi.Demo.Models;

public interface IView
{
    OvoNotificationManager NotificationManager { get; }

    OvoToastManager ToastManager { get; }
}

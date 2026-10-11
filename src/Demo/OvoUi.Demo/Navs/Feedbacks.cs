using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Feedbacks
{
    public static readonly List<Page> FeedbacksList =
    [
        Page.View<ContentDialogPage>("ContentDialog"),
        Page.View<DialogPage>("Dialog"),
        Page.View<DrawerPage>("Drawer"),
        Page.View<MessageBoxPage>("MessageBox"),
        Page.View<NotificationPage>("Notification"),
        Page.View<ToastPage>("Toast")
    ];
}

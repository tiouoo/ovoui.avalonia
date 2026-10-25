using System.Collections.Generic;
using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Feedbacks
{
    public static readonly List<Page> FeedbacksList =
    [
        Page.View<ContentDialogPage>("ContentDialog"),
        Page.View<DataValidationErrorsPage>("DataValidationErrors"),
        Page.View<DialogPage>("Dialog"),
        Page.View<DrawerPage>("Drawer"),
        Page.View<LoadingPage>("Loading"),
        Page.View<MessageBoxPage>("MessageBox"),
        Page.View<NotificationPage>("Notification"),
        Page.View<PopConfirmPage>("PopConfirm"),
        Page.View<RefreshContainerPage>("RefreshContainer"),
        Page.View<SkeletonPage>("Skeleton"),
        Page.View<ToastPage>("Toast")
    ];
}
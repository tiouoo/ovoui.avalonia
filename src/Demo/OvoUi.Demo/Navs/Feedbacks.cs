using OvoUi.Demo.Models;
using OvoUi.Demo.Views.Pages;

namespace OvoUi.Demo.Navs;

public static class Feedbacks
{
    public static readonly List<Page> FeedbacksList =
    [
        new() { Title = "ContentDialog", Content = new BlankPage() },
        new() { Title = "DataValidationErrors", Content = new BlankPage() },
        new() { Title = "Dialog", Content = new BlankPage() },
        new() { Title = "DisableContainer", Content = new BlankPage() },
        new() { Title = "Drawer", Content = new BlankPage() },
        new() { Title = "LiquidLikeDecoratorPage", Content = new BlankPage() },
        new() { Title = "Loading", Content = new BlankPage() },
        new() { Title = "MessageBox", Content = new BlankPage() },
        new() { Title = "Notification", Content = new BlankPage() },
        new() { Title = "OvoNotification", Content = new BlankPage() },
        new() { Title = "OvoToast", Content = new BlankPage() },
        new() { Title = "PopConfirm", Content = new BlankPage() },
        new() { Title = "RefreshContainer", Content = new BlankPage() },
        new() { Title = "Shimmer", Content = new BlankPage() },
        new() { Title = "Skeleton", Content = new BlankPage() },
        new() { Title = "Toast", Content = new BlankPage() }
    ];
}

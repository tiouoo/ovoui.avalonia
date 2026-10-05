using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using OvoUi.Controls;
using OvoUi.Demo.Models;

namespace OvoUi.Demo.Views;

public partial class MainWindow : OvoWindow, IView
{
    public MainWindow()
    {
        InitializeComponent();
        NotificationManager = new OvoNotificationManager(this);
        ToastManager = new OvoToastManager(this);
    }

    public OvoNotificationManager NotificationManager { get; }

    public OvoToastManager ToastManager { get; }

    private void ThemeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is { } application)
            application.RequestedThemeVariant = application.ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
    }
}

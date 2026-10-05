using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using OvoUi.Controls;
using OvoUi.Demo.Models;

namespace OvoUi.Demo.Views;

public partial class MainView : OvoView, IView
{
    private OvoNotificationManager? _notificationManager;
    private OvoToastManager? _toastManager;

    public MainView()
    {
        InitializeComponent();
        KeyBindings.Add(new KeyBinding
        {
            Gesture = KeyGesture.Parse("Ctrl+Q"),
            Command = new ActionCommand(ToggleTheme)
        });
    }

    private static void ToggleTheme()
    {
        if (Application.Current is { } application)
            application.RequestedThemeVariant = application.ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
    }

    public OvoNotificationManager NotificationManager =>
        _notificationManager ??= new OvoNotificationManager(TopLevel.GetTopLevel(this));

    public OvoToastManager ToastManager =>
        _toastManager ??= new OvoToastManager(TopLevel.GetTopLevel(this));
}

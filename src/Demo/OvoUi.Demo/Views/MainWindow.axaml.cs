using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using OvoUi.Common.Helpers;
using OvoUi.Controls;
using OvoUi.Demo.Models;

namespace OvoUi.Demo.Views;

public partial class MainWindow : OvoWindow, IView
{
    private static readonly MacOsWindowControlsOptions MacOsMainWindowControlsOptions = new()
    {
        OriginX = 17,
        OriginY = -1,
        Spacing = 22
    };

    private bool _macOsWindowControlsRefreshQueued;

    public MainWindow()
    {
        InitializeComponent();
        NotificationManager = new OvoNotificationManager(this);
        ToastManager = new OvoToastManager(this);

        if (OperatingSystem.IsMacOS())
        {
            TitleBarVisibilityOnFullScreen = TitleBarVisibilityMode.Visible;
            TitleBarAnimationEnabled = false;
            IsFullScreenTransitionAnimationEnabled = false;
            OvoTitleBar.SetIsCloseButtonVisible(this, false);
            OvoTitleBar.SetIsMinimizeButtonVisible(this, false);
            OvoTitleBar.SetIsMaximizeButtonVisible(this, false);
            OvoTitleBar.SetIsFullScreenButtonVisible(this, false);
            OvoTitleBar.SetIsPinButtonVisible(this, true);
            OvoTitleBar.SetControlButtonMargin(this, new Thickness(0, 0, 12, 0));
            UpdateMacOsTitleBarLayout();
        }
    }

    public OvoNotificationManager NotificationManager { get; }

    public OvoToastManager ToastManager { get; }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        QueueMacOsWindowControlsRefresh();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        QueueMacOsWindowControlsRefresh();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty)
        {
            UpdateMacOsTitleBarLayout();
            QueueMacOsWindowControlsRefresh();
        }
    }

    private void UpdateMacOsTitleBarLayout()
    {
        if (!OperatingSystem.IsMacOS() || BrandContent is null)
            return;

        BrandContent.Margin = WindowState == WindowState.FullScreen
            ? new Thickness(12, 0)
            : new Thickness(92, 0, 12, 0);
    }

    private void QueueMacOsWindowControlsRefresh()
    {
        if (!OperatingSystem.IsMacOS() || _macOsWindowControlsRefreshQueued)
            return;

        _macOsWindowControlsRefreshQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _macOsWindowControlsRefreshQueued = false;
            MacOsWindowControls.TryApply(this, MacOsMainWindowControlsOptions, out _);
        }, DispatcherPriority.Background);
    }

    private void ThemeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is { } application)
            application.RequestedThemeVariant = application.ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
    }
}

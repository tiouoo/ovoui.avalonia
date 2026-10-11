using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using OvoUi.Platform.MacOS;
using OvoUi.Common.Language;
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
        if (App.JumpListLaunchTarget is { } jumpListTarget)
            Title = $"OvoUi Demo - Jump List: {jumpListTarget}";
        TextOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        TextOptions.SetTextHintingMode(this, TextHintingMode.Light);
        TextOptions.SetBaselinePixelAlignment(this, BaselinePixelAlignment.Aligned);
        NotificationManager = new OvoNotificationManager(this);
        ToastManager = new OvoToastManager(this);
        ConfigureBackgroundMenu();

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

    private void ConfigureBackgroundMenu()
    {
        BackgroundMicaItem.IsVisible = OperatingSystem.IsWindows();
        BackgroundAcrylicItem.IsVisible = OperatingSystem.IsWindows();
        BackgroundBlurItem.IsVisible = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux();
    }

    private void SetThemeDefault(object? sender, RoutedEventArgs e) => SetTheme(ThemeVariant.Default);

    private void SetThemeLight(object? sender, RoutedEventArgs e) => SetTheme(ThemeVariant.Light);

    private void SetThemeDark(object? sender, RoutedEventArgs e) => SetTheme(ThemeVariant.Dark);

    private static void SetTheme(ThemeVariant theme)
    {
        if (Application.Current is { } application)
            application.RequestedThemeVariant = theme;
    }

    private void SetLanguageEnglish(object? sender, RoutedEventArgs e) => LangManager.SetLanguage(Languages.en_us);

    private void SetLanguageChinese(object? sender, RoutedEventArgs e) => LangManager.SetLanguage(Languages.zh_cn);

    private void SetBackgroundDefault(object? sender, RoutedEventArgs e)
    {
        ClearValue(TransparencyLevelHintProperty);
        ClearValue(BackgroundProperty);
        ClearValue(TransparencyBackgroundFallbackProperty);
        SetTranslucentOutlineOpacity(1);
    }

    private void SetBackgroundMica(object? sender, RoutedEventArgs e) =>
        SetBackgroundEffect(WindowTransparencyLevel.Mica);

    private void SetBackgroundAcrylic(object? sender, RoutedEventArgs e) =>
        SetBackgroundEffect(WindowTransparencyLevel.AcrylicBlur);

    private void SetBackgroundBlur(object? sender, RoutedEventArgs e) =>
        SetBackgroundEffect(WindowTransparencyLevel.Blur);

    private void SetBackgroundTransparent(object? sender, RoutedEventArgs e) =>
        SetBackgroundEffect(WindowTransparencyLevel.Transparent);

    private void SetBackgroundEffect(WindowTransparencyLevel transparencyLevel)
    {
        TransparencyLevelHint = [transparencyLevel];
        Background = Brushes.Transparent;
        TransparencyBackgroundFallback = Brushes.Transparent;
        SetTranslucentOutlineOpacity(0.35);
    }

    private static void SetTranslucentOutlineOpacity(double opacity)
    {
        if (Application.Current is { } application)
            application.Resources["TranslucentOpacity"] = opacity;
    }

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

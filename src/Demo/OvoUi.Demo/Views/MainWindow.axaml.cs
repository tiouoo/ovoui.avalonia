using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using OvoUi.Common.Language;
using OvoUi.Controls;
using OvoUi.Demo.Models;

namespace OvoUi.Demo.Views;

public partial class MainWindow : OvoWindow, IView
{
    public MainWindow()
    {
        InitializeComponent();
        TextOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        TextOptions.SetTextHintingMode(this, TextHintingMode.Light);
        TextOptions.SetBaselinePixelAlignment(this, BaselinePixelAlignment.Aligned);
        NotificationManager = new OvoNotificationManager(this);
        ToastManager = new OvoToastManager(this);
        ConfigureBackgroundMenu();

    }

    public OvoNotificationManager NotificationManager { get; }

    public OvoToastManager ToastManager { get; }

    private void ConfigureBackgroundMenu()
    {
        BackgroundMicaItem.IsVisible = OperatingSystem.IsWindows();
        BackgroundAcrylicItem.IsVisible = OperatingSystem.IsWindows();
        BackgroundBlurItem.IsVisible =
            OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux();
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

    private void ThemeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is { } application)
            application.RequestedThemeVariant = application.ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
    }
}
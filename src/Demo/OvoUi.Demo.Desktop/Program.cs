using System;
using Avalonia;
using Avalonia.Dialogs;
using OvoUi.Platform.Windows;

namespace OvoUi.Demo.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        WindowsAppIdentity.TrySetCurrentProcessAppId("cc.tiouo.OvoUi.Demo", out _);
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .With(new Win32PlatformOptions
        {
            RenderingMode = [Win32RenderingMode.AngleEgl],
            CompositionMode = [Win32CompositionMode.WinUIComposition]
        })
        .WithInterFont()
        .UseManagedSystemDialogs()
        .LogToTrace();
}

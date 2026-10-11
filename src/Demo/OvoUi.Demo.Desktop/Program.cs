using System;
using Avalonia;
using Avalonia.Dialogs;
using OvoUi.Platform.Windows;

namespace OvoUi.Demo.Desktop;

sealed class Program
{
    private const string AppUserModelId = "cc.tiouo.OvoUi.Demo";

    [STAThread]
    public static void Main(string[] args)
    {
        WindowsJumpList.TrySetCurrentProcessAppId(AppUserModelId, out _);
        var jumpListArgumentIndex = Array.IndexOf(args, "--jump-list-demo");
        if (jumpListArgumentIndex >= 0 && jumpListArgumentIndex + 1 < args.Length)
            App.JumpListLaunchTarget = args[jumpListArgumentIndex + 1];
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
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

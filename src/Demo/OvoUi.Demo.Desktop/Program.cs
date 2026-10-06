using System;
using Avalonia;
using Avalonia.Dialogs;
using HotAvalonia;

namespace OvoUi.Demo.Desktop;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
#if DEBUG
            .UseHotReload()
#endif
            .UsePlatformDetect()
            .WithInterFont()
            .UseManagedSystemDialogs()
            .LogToTrace();
}
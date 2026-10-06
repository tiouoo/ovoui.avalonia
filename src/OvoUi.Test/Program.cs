using Avalonia;
using System;
using Avalonia.Dialogs;
using HotAvalonia;

namespace OvoUi.Test;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .UseHotReload()
#endif
            .WithInterFont()
            .UseManagedSystemDialogs()
            .LogToTrace();
}

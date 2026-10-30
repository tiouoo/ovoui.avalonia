using Avalonia;
using OvoUi.Platform.Windows;

namespace OvoUi.Demo.Classic;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        WindowsJumpList.TrySetCurrentProcessAppId("cc.tiouo.OvoUi.Demo.Classic", out _);
        var index = Array.IndexOf(args, "--jump-list-demo");
        if (index >= 0 && index + 1 < args.Length)
            App.JumpListLaunchTarget = args[index + 1];
        AppBuilder.Configure<App>().UsePlatformDetect()
            .With(new Win32PlatformOptions
            {
                RenderingMode = [Win32RenderingMode.AngleEgl],
                CompositionMode = [Win32CompositionMode.WinUIComposition]
            })
            .WithInterFont().LogToTrace().StartWithClassicDesktopLifetime(args);
    }
}

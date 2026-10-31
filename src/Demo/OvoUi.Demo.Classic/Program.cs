using Avalonia;

namespace OvoUi.Demo.Classic;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        AppBuilder.Configure<App>().UsePlatformDetect()
            .With(new Win32PlatformOptions
            {
                RenderingMode = [Win32RenderingMode.AngleEgl],
                CompositionMode = [Win32CompositionMode.WinUIComposition]
            })
            .WithInterFont().LogToTrace().StartWithClassicDesktopLifetime(args);
    }
}

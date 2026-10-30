using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace OvoUi.Demo.Classic;

public partial class App : Application
{
    public static string? JumpListLaunchTarget { get; set; }
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            desktop.MainWindow = window;
            if (desktop.Args?.Contains("--smoke-test") == true)
                window.Opened += async (_, _) => await window.SmokeTestAsync(desktop);
        }
        base.OnFrameworkInitializationCompleted();
    }
}

using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;

namespace OvoUi.Demo.Browser;

internal sealed partial class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
        .WithInterFont()
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
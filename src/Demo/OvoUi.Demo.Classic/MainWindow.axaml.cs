using Avalonia.Controls;
using OvoUi.Demo.Classic.Pages;

namespace OvoUi.Demo.Classic;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (App.JumpListLaunchTarget is { } target)
            Title = $"OvoUi Classic Demo - Jump List: {target}";
        Examples.ItemsSource = new TabItem[]
        {
            Example<DataGridPage>("DataGrid"),
            Example<MacOsWindowControlsPage>("macOS controls"),
            Example<WindowsSystemMenuPage>("Windows menu / jump list"),
            Example<WindowCompositionMaterialPage>("Composition material"),
            Example<WindowContentInjectionPage>("Window injection"),
            Example<MemoryOptimizationPage>("Memory")
        };
    }

    private static TabItem Example<T>(string title) where T : UserControl, new() => new()
    {
        Header = title,
        Content = new LazyExample<T>()
    };

    internal async Task SmokeTestAsync(Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime lifetime)
    {
        var report = new List<string>();
        var exitCode = 0;
        try
        {
            for (var index = 0; index < Examples.ItemCount; index++)
            {
                Examples.SelectedIndex = index;
                UpdateLayout();
                await Task.Delay(30);
                report.Add($"PASS {((TabItem)Examples.Items[index]!).Header}");
            }
        }
        catch (Exception exception)
        {
            report.Add($"FAIL {exception}");
            exitCode = 1;
        }
        finally
        {
            File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "smoke-test.log"), report);
            lifetime.Shutdown(exitCode);
        }
    }

    private sealed class LazyExample<T> : Decorator where T : UserControl, new()
    {
        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            Child ??= new T();
        }
    }
}

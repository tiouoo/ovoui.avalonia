using Avalonia.Controls;
using Avalonia.Media;
using OvoUi.Controls;
using OvoUi.Demo.Classic.Pages;

namespace OvoUi.Demo.Classic;

public partial class MainWindow : OvoWindow
{
    public MainWindow()
    {
        InitializeComponent();
        if (App.JumpListLaunchTarget is { } target)
            Title = $"OvoUi Classic Demo - Jump List: {target}";
        Examples.ItemsSource = new TabItem[]
        {
            Example<DataGridPage>("DataGrid"),
            Example<WindowsJumpListPage>("Windows Jump List"),
            Example<WindowCompositionMaterialPage>("Composition material")
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
            UpdateLayout();
            if (TitleBar is null)
                throw new InvalidOperationException("OvoWindow title bar template was not loaded.");
            report.Add("PASS OvoWindow title bar");
            if (this.FindResource("OvoFontFamily") is not FontFamily fontFamily)
                throw new InvalidOperationException("OvoUi.Fonts resources were not loaded.");
            foreach (var weight in new[] { FontWeight.Normal, FontWeight.Bold })
            {
                if (!FontManager.Current.TryGetGlyphTypeface(new Typeface(fontFamily, weight: weight), out var glyphTypeface))
                    throw new InvalidOperationException($"Cannot load embedded OvoUi font ({weight}).");
                report.Add($"PASS OvoUi.Fonts {weight}: {glyphTypeface.FamilyName}");
            }
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

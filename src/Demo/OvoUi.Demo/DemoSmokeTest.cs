using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;
using OvoUi.Common.Language;
using OvoUi.Demo.Models;
using OvoUi.Demo.Views;

namespace OvoUi.Demo;

internal static class DemoSmokeTest
{
    // Exercise the published executable: native compilation alone cannot detect missing XAML resources.
    public static async Task RunAsync(MainWindow window, IClassicDesktopStyleApplicationLifetime lifetime)
    {
        var report = new List<string>();
        var exitCode = 0;
        try
        {
            var demoView = window.GetVisualDescendants().OfType<DemoView>().Single();
            var viewModel = (DemoViewModel)demoView.DataContext!;
            var presenter = demoView.GetVisualDescendants().OfType<TransitioningContentControl>().Single();
            presenter.PageTransition = null;
            LangManager.SetLanguage(Languages.zh_cn);
            LangManager.SetLanguage(Languages.en_us);
            foreach (var page in DemoViewModel.Pages.SelectMany(page => page.Children is { } children
                         ? children.AsEnumerable() : new[] { page }))
            {
                viewModel.SelectedPage = page;
                window.UpdateLayout();
                await Task.Delay(30);
                report.Add($"PASS {page.Title}");
            }
            await PlatformSmokeTest.RunAsync(window, report);
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
}

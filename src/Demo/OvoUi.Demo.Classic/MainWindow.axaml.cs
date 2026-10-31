using Avalonia.Controls;
using Avalonia.Media;
using OvoUi.Controls;
using OvoUi.Demo.Classic.Models;
using OvoUi.Demo.Classic.Views;
using Avalonia.VisualTree;

namespace OvoUi.Demo.Classic;

public partial class MainWindow : OvoWindow
{
    public MainWindow()
    {
        InitializeComponent();
        TextOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        TextOptions.SetTextHintingMode(this, TextHintingMode.Light);
        TextOptions.SetBaselinePixelAlignment(this, BaselinePixelAlignment.Aligned);
    }

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
            var view = this.GetVisualDescendants().OfType<DemoView>().Single();
            var model = (DemoViewModel)view.DataContext!;
            view.FindControl<TransitioningContentControl>("PagePresenter")!.PageTransition = null;
            foreach (var page in DemoViewModel.Pages.SelectMany(group => group.Children!))
            {
                model.SelectedPage = page;
                UpdateLayout();
                await Task.Delay(30);
                report.Add($"PASS {page.Title}");
            }
            var navigation = view.FindControl<NavMenu>("NavMenu")!;
            navigation.SearchText = "DataGrid";
            UpdateLayout();
            await Task.Delay(100);
            if (!navigation.GetVisualDescendants().OfType<NavMenuItem>()
                    .Any(item => item.IsEffectivelyVisible && item.Header?.ToString() == "DataGrid"))
                throw new InvalidOperationException("Navigation search did not retain the matching page.");
            report.Add("PASS navigation search");
            navigation.SearchText = "";
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

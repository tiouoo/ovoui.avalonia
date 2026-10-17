using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using OvoUi.Demo.Views;
using OvoUi.Platform.Windows;

namespace OvoUi.Demo.Pages;

public partial class WindowContentInjectionPage : UserControl
{
    private InjectedContentWindow? _contentWindow;
    private WindowContentInjectionSession? _session;

    public WindowContentInjectionPage()
    {
        InitializeComponent();
        DataContext = this;
        AttachedToVisualTree += (_, _) => RefreshWindowList();
        DetachedFromVisualTree += (_, _) => RemoveInjectedContent();
    }

    public bool IsWindows { get; } = OperatingSystem.IsWindows();

    public string SourceCode { get; } = """
                                        var targets = WindowContentInjection.GetAvailableWindows();
                                        var contentWindow = new Window { Content = new MyOverlayContent() };
                                        contentWindow.Show();

                                        if (WindowContentInjection.TryInject(contentWindow, targets[0],
                                                out var session, out var error,
                                                new WindowContentInjectionOptions { IsInteractive = true }))
                                        {
                                            // Keep the session alive. Dispose it to restore the original window state.
                                        }
                                        """;

    private void RefreshWindows(object? sender, RoutedEventArgs e) => RefreshWindowList();

    private void RefreshWindowList()
    {
        if (!IsWindows) return;
        var ownProcessId = Environment.ProcessId;
        var windows = WindowContentInjection.GetAvailableWindows()
            .Where(window => window.ProcessId != ownProcessId).ToArray();
        WindowPicker.ItemsSource = windows;
        WindowPicker.SelectedIndex = windows.Length > 0 ? 0 : -1;
        StatusLabel.Text = windows.Length == 0
            ? "No compatible visible windows were found."
            : $"Found {windows.Length} target windows.";
    }

    private void InjectContent(object? sender, RoutedEventArgs e)
    {
        if (WindowPicker.SelectedItem is not NativeWindowInfo target)
        {
            StatusLabel.Text = "Select a target window first.";
            return;
        }

        RemoveInjectedContent();
        _contentWindow = new InjectedContentWindow();
        _contentWindow.Show();
        Dispatcher.UIThread.Post(() =>
        {
            if (_contentWindow is null) return;
            var options = new WindowContentInjectionOptions
            {
                IsInteractive = InteractiveCheckBox.IsChecked == true
            };
            if (WindowContentInjection.TryInject(_contentWindow, target, out _session, out var error, options))
                StatusLabel.Text = $"Content injected into '{target.Title}'. Use Remove content to detach it.";
            else
            {
                StatusLabel.Text = error ?? "The content could not be injected.";
                _contentWindow.Close();
                _contentWindow = null;
            }
        }, DispatcherPriority.Loaded);
    }

    private void RemoveContent(object? sender, RoutedEventArgs e) => RemoveInjectedContent();

    private void RemoveInjectedContent()
    {
        _session?.Dispose();
        _session = null;
        _contentWindow?.Close();
        _contentWindow = null;
    }
}
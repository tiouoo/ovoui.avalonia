using Avalonia.Controls;
using Avalonia.Interactivity;
using OvoUi.Platform.Windows;

namespace OvoUi.Demo.Classic.Pages;

public partial class WindowsJumpListPage : UserControl
{
    private const string DemoAppId = "cc.tiouo.OvoUi.Demo.Classic";

    public WindowsJumpListPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public bool IsWindows { get; } = OperatingSystem.IsWindows();
    public string JumpListSourceCode { get; } = """
                                                var items = new[]
                                                {
                                                    new WindowsJumpListItem(
                                                        "Tasks",
                                                        "Open overview",
                                                        "Launch the OvoUi demo overview",
                                                        "--jump-list-demo overview"),
                                                    new WindowsJumpListItem(
                                                        "Examples",
                                                        "Open controls",
                                                        "Launch the OvoUi controls demo",
                                                        "--jump-list-demo controls")
                                                };

                                                WindowsJumpList.TryApply(
                                                    "cc.example.MyApplication",
                                                    items,
                                                    out var error);
                                                """;

    private void ApplyJumpList(object? sender, RoutedEventArgs e)
    {
        var items = new[]
        {
            new WindowsJumpListItem(
                "Demo tasks",
                "Open OvoUi Demo",
                "Launch another OvoUi demo window",
                "--jump-list-demo home"),
            new WindowsJumpListItem(
                "Demo tasks",
                "Open platform examples",
                "Launch the OvoUi demo for platform examples",
                "--jump-list-demo platform"),
            new WindowsJumpListItem(
                "Resources",
                "Open control gallery",
                "Launch the OvoUi control gallery",
                "--jump-list-demo controls")
        };

        JumpListStatusLabel.Text = WindowsJumpList.TryApply(DemoAppId, items, out var error)
            ? "Jump List applied. Right-click the taskbar icon normally to view it."
            : error ?? "The Jump List could not be applied.";
    }

    private void ClearJumpList(object? sender, RoutedEventArgs e)
    {
        JumpListStatusLabel.Text = WindowsJumpList.TryClear(DemoAppId, out var error)
            ? "Jump List cleared. Windows may take a moment to refresh the taskbar menu."
            : error ?? "The Jump List could not be cleared.";
    }
}
using Avalonia.Controls;
using Avalonia.Interactivity;
using OvoUi.Platform.Windows;

namespace OvoUi.Demo.Pages;

public partial class WindowsSystemMenuPage : UserControl
{
    private const string DemoAppId = "cc.tiouo.OvoUi.Demo";
    private readonly List<WindowsSystemMenuItem> _menuItems = [];
    private int _commandNumber;

    public WindowsSystemMenuPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public bool IsWindows { get; } = OperatingSystem.IsWindows();

    public string SourceCode { get; } = """
                                        private void AddCommand(object? sender, RoutedEventArgs e)
                                        {
                                            var window = (Window)TopLevel.GetTopLevel(this)!;
                                            if (WindowsSystemMenu.TryAddItem(
                                                    window,
                                                    "Refresh data",
                                                    RefreshData,
                                                    out var item,
                                                    out var error))
                                            {
                                                _menuItems.Add(item!);
                                            }
                                        }

                                        private void ShowMenu(object? sender, RoutedEventArgs e)
                                        {
                                            var window = (Window)TopLevel.GetTopLevel(this)!;
                                            WindowsSystemMenu.TryShow(window, out _);
                                        }
                                        """;

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

    private void AddCommand(object? sender, RoutedEventArgs e)
    {
        if (!TryGetWindow(out var window))
            return;

        var number = ++_commandNumber;
        var text = $"Demo command {number}";
        if (WindowsSystemMenu.TryAddItem(
                window,
                text,
                () => StatusLabel.Text = $"{text} invoked at {DateTime.Now:T}.",
                out var item,
                out var error))
        {
            _menuItems.Add(item!);
            StatusLabel.Text = $"Added {text}. Click Show menu to inspect it now.";
            UpdateItemCount();
            return;
        }

        StatusLabel.Text = error ?? "The command could not be added.";
    }

    private void AddSeparator(object? sender, RoutedEventArgs e)
    {
        if (!TryGetWindow(out var window))
            return;

        if (WindowsSystemMenu.TryAddSeparator(window, out var item, out var error))
        {
            _menuItems.Add(item!);
            StatusLabel.Text = "Added a separator.";
            UpdateItemCount();
            return;
        }

        StatusLabel.Text = error ?? "The separator could not be added.";
    }

    private void RemoveLast(object? sender, RoutedEventArgs e)
    {
        if (_menuItems.Count == 0)
        {
            StatusLabel.Text = "There are no custom items to remove.";
            return;
        }

        var removed = _menuItems[^1].Remove();
        _menuItems.RemoveAt(_menuItems.Count - 1);
        StatusLabel.Text = removed ? "Removed the last custom item." : "The item was already removed.";
        UpdateItemCount();
    }

    private void ClearAll(object? sender, RoutedEventArgs e)
    {
        if (!TryGetWindow(out var window))
            return;

        StatusLabel.Text = WindowsSystemMenu.TryClear(window, out var error)
            ? "Removed all custom items."
            : error ?? "The custom items could not be cleared.";
        _menuItems.Clear();
        UpdateItemCount();
    }

    private void ShowMenu(object? sender, RoutedEventArgs e)
    {
        if (!TryGetWindow(out var window))
            return;

        if (!WindowsSystemMenu.TryShow(window, out var error))
            StatusLabel.Text = error ?? "The system menu could not be shown.";
    }

    private bool TryGetWindow(out Window window)
    {
        window = TopLevel.GetTopLevel(this) as Window ?? null!;
        if (window is not null)
            return true;

        StatusLabel.Text = "The page is not attached to a desktop window.";
        return false;
    }

    private void UpdateItemCount() =>
        ItemCountLabel.Text = $"{_menuItems.Count} custom item(s) are registered.";

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
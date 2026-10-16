using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using OvoUi.Tab;

namespace OvoUi.Demo.Pages;

public partial class TabWindowPage : UserControl
{
    private static readonly Color[] AccentColors =
    [
        Color.Parse("#1677ff"),
        Color.Parse("#13a8a8"),
        Color.Parse("#d46b08"),
        Color.Parse("#c41d7f"),
        Color.Parse("#389e0d")
    ];

    private readonly OvoTabService _tabService = new();
    private int _accentIndex;
    private int _tabNumber;

    public TabWindowPage()
    {
        InitializeComponent();
        TabHost.TabService = _tabService;
        TabHost.NewTabFactory = CreateTab;
        OpenWindowButton.IsVisible = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime;

        TabHost.AddTab(CreateTab("Dashboard", OvoTabIcons.Reorder));
        TabHost.AddTab(CreateTab("Activity", OvoTabIcons.Transfer), select: false);
        TabHost.AddTab(CreateTab("Deployment logs", OvoTabIcons.Detach), select: false);
        TabHost.AddTab(CreateTab("Settings", OvoTabIcons.Add), select: false);
    }

    private OvoTabItem CreateTab() => CreateTab($"Document {++_tabNumber}", OvoTabIcons.Add);

    private OvoTabItem CreateTab(string title, StreamGeometry icon)
    {
        var accent = AccentColors[_accentIndex++ % AccentColors.Length];
        return new OvoTabItem(title, CreateContent(title, accent))
        {
            Icon = icon,
            MinimumWidth = 145
        };
    }

    private static Control CreateContent(string title, Color accent)
    {
        var accentBar = new Border
        {
            Width = 72,
            Height = 4,
            CornerRadius = new CornerRadius(2),
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(accent)
        };
        Grid.SetRow(accentBar, 2);

        var timestamp = new TextBlock
        {
            Margin = new Thickness(0, 18, 0, 0),
            Text = DateTimeOffset.Now.ToString("F"),
            Opacity = 0.65
        };
        Grid.SetRow(timestamp, 3);

        return new Grid
        {
            Margin = new Thickness(24),
            RowDefinitions = RowDefinitions.Parse("Auto,12,Auto,*"),
            Children =
            {
                new TextBlock
                {
                    Text = title,
                    FontSize = 24,
                    FontWeight = FontWeight.SemiBold
                },
                accentBar,
                timestamp
            }
        };
    }

    private void AddTabs_OnClick(object? sender, RoutedEventArgs e)
    {
        for (var i = 0; i < 4; i++) TabHost.AddTab(CreateTab(), select: i == 3);
    }

    private void OpenWindow_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime) return;

        var window = new OvoTabWindow(_tabService)
        {
            Title = "OvoUi Tab Workspace",
            NewTabFactory = CreateTab,
            CreateTabWhenEmpty = true
        };
        window.AddTab(CreateTab("Welcome", OvoTabIcons.Transfer));
        window.AddTab(CreateTab("Release notes", OvoTabIcons.Reorder), select: false);

        window.Show();
    }
}

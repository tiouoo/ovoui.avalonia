using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OvoUi.Demo.Pages;

public partial class SplitViewPage : UserControl
{
    public SplitViewPage()
    {
        InitializeComponent();
        DataContext = new SplitViewDemoViewModel();
    }
}

public partial class SplitViewDemoViewModel : ObservableObject
{
    public IReadOnlyList<string> Items { get; } =
        ["Overview", "Projects", "Activity", "Settings"];

    public IReadOnlyList<SplitViewDisplayMode> DisplayModes { get; } =
    [
        SplitViewDisplayMode.Inline,
        SplitViewDisplayMode.CompactInline,
        SplitViewDisplayMode.Overlay,
        SplitViewDisplayMode.CompactOverlay,
    ];

    public IReadOnlyList<SplitViewPanePlacement> Placements { get; } =
    [
        SplitViewPanePlacement.Left,
        SplitViewPanePlacement.Right,
        SplitViewPanePlacement.Top,
        SplitViewPanePlacement.Bottom,
    ];

    public string SourceCode { get; } = """
                                        public partial class SplitViewPage : UserControl
                                        {
                                            public SplitViewPage()
                                            {
                                                InitializeComponent();
                                                DataContext = new SplitViewDemoViewModel();
                                            }
                                        }

                                        public partial class SplitViewDemoViewModel : ObservableObject
                                        {
                                            public IReadOnlyList<string> Items { get; } =
                                                ["Overview", "Projects", "Activity", "Settings"];

                                            public IReadOnlyList<SplitViewDisplayMode> DisplayModes { get; } =
                                            [
                                                SplitViewDisplayMode.Inline,
                                                SplitViewDisplayMode.CompactInline,
                                                SplitViewDisplayMode.Overlay,
                                                SplitViewDisplayMode.CompactOverlay,
                                            ];

                                            public IReadOnlyList<SplitViewPanePlacement> Placements { get; } =
                                            [
                                                SplitViewPanePlacement.Left,
                                                SplitViewPanePlacement.Right,
                                                SplitViewPanePlacement.Top,
                                                SplitViewPanePlacement.Bottom,
                                            ];

                                            [ObservableProperty] private bool _isPaneOpen = true;
                                            [ObservableProperty] private bool _useLightDismissOverlayMode = true;
                                            [ObservableProperty] private double _compactPaneLength = 48;
                                            [ObservableProperty] private double _openPaneLength = 240;
                                            [ObservableProperty] private SplitViewDisplayMode _selectedDisplayMode = SplitViewDisplayMode.CompactInline;
                                            [ObservableProperty] private SplitViewPanePlacement _selectedPlacement = SplitViewPanePlacement.Left;

                                            [RelayCommand]
                                            private void TogglePane() => IsPaneOpen = !IsPaneOpen;
                                        }
                                        """;

    [ObservableProperty] private bool _isPaneOpen = true;
    [ObservableProperty] private bool _useLightDismissOverlayMode = true;
    [ObservableProperty] private double _compactPaneLength = 48;
    [ObservableProperty] private double _openPaneLength = 240;
    [ObservableProperty] private SplitViewDisplayMode _selectedDisplayMode = SplitViewDisplayMode.CompactInline;
    [ObservableProperty] private SplitViewPanePlacement _selectedPlacement = SplitViewPanePlacement.Left;

    [RelayCommand]
    private void TogglePane() => IsPaneOpen = !IsPaneOpen;
}
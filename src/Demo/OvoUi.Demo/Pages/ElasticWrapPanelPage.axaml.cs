using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OvoUi.Demo.Pages;

public partial class ElasticWrapPanelPage : UserControl
{
    public ElasticWrapPanelPage()
    {
        InitializeComponent();
        DataContext = new ElasticWrapPanelDemoViewModel();
    }
}

public partial class ElasticWrapPanelDemoViewModel : ObservableObject
{
    public IReadOnlyList<Orientation> Orientations { get; } = Enum.GetValues<Orientation>();
    public IReadOnlyList<ScrollBarVisibility> ScrollBarVisibilities { get; } = Enum.GetValues<ScrollBarVisibility>();
    public IReadOnlyList<HorizontalAlignment> HorizontalAlignments { get; } = Enum.GetValues<HorizontalAlignment>();
    public IReadOnlyList<VerticalAlignment> VerticalAlignments { get; } = Enum.GetValues<VerticalAlignment>();

    [ObservableProperty] private Orientation _selectedOrientation = Orientation.Horizontal;
    [ObservableProperty] private ScrollBarVisibility _horizontalVisibility = ScrollBarVisibility.Auto;
    [ObservableProperty] private ScrollBarVisibility _verticalVisibility = ScrollBarVisibility.Auto;
    [ObservableProperty] private bool _isFillHorizontal;
    [ObservableProperty] private bool _isFillVertical;
    [ObservableProperty] private double _itemWidth = 100;
    [ObservableProperty] private double _itemHeight = 54;
    [ObservableProperty] private double _itemSpacing = 8;
    [ObservableProperty] private double _lineSpacing = 8;
    [ObservableProperty] private bool _autoWidth = true;
    [ObservableProperty] private bool _autoHeight = true;
    [ObservableProperty] private double _manualItemSelfWidth = 240;
    [ObservableProperty] private double _manualItemSelfHeight = 54;
    [ObservableProperty] private HorizontalAlignment _fixedHorizontalAlignment = HorizontalAlignment.Right;
    [ObservableProperty] private VerticalAlignment _fixedVerticalAlignment = VerticalAlignment.Stretch;

    public double ItemSelfWidth => AutoWidth ? double.NaN : ManualItemSelfWidth;
    public double ItemSelfHeight => AutoHeight ? double.NaN : ManualItemSelfHeight;

    partial void OnAutoWidthChanged(bool value)
    {
        OnPropertyChanged(nameof(ItemSelfWidth));
    }

    partial void OnAutoHeightChanged(bool value)
    {
        OnPropertyChanged(nameof(ItemSelfHeight));
    }

    partial void OnManualItemSelfWidthChanged(double value) => OnPropertyChanged(nameof(ItemSelfWidth));
    partial void OnManualItemSelfHeightChanged(double value) => OnPropertyChanged(nameof(ItemSelfHeight));

    public string SourceCode { get; } = """
                                        public partial class ElasticWrapPanelDemoViewModel : ObservableObject
                                        {
                                            [ObservableProperty] private Orientation _selectedOrientation = Orientation.Horizontal;
                                            [ObservableProperty] private ScrollBarVisibility _horizontalVisibility = ScrollBarVisibility.Auto;
                                            [ObservableProperty] private ScrollBarVisibility _verticalVisibility = ScrollBarVisibility.Auto;
                                            [ObservableProperty] private bool _isFillHorizontal;
                                            [ObservableProperty] private bool _isFillVertical;
                                            [ObservableProperty] private double _itemWidth = 100;
                                            [ObservableProperty] private double _itemHeight = 54;
                                            [ObservableProperty] private double _itemSpacing = 8;
                                            [ObservableProperty] private double _lineSpacing = 8;
                                            [ObservableProperty] private bool _autoWidth = true;
                                            [ObservableProperty] private bool _autoHeight = true;
                                            [ObservableProperty] private double _manualItemSelfWidth = 240;
                                            [ObservableProperty] private double _manualItemSelfHeight = 54;
                                            [ObservableProperty] private HorizontalAlignment _fixedHorizontalAlignment = HorizontalAlignment.Right;
                                            [ObservableProperty] private VerticalAlignment _fixedVerticalAlignment = VerticalAlignment.Stretch;

                                            public double ItemSelfWidth => AutoWidth ? double.NaN : ManualItemSelfWidth;
                                            public double ItemSelfHeight => AutoHeight ? double.NaN : ManualItemSelfHeight;

                                            partial void OnAutoWidthChanged(bool value) => OnPropertyChanged(nameof(ItemSelfWidth));
                                            partial void OnAutoHeightChanged(bool value) => OnPropertyChanged(nameof(ItemSelfHeight));
                                            partial void OnManualItemSelfWidthChanged(double value) => OnPropertyChanged(nameof(ItemSelfWidth));
                                            partial void OnManualItemSelfHeightChanged(double value) => OnPropertyChanged(nameof(ItemSelfHeight));
                                        }
                                        """;
}
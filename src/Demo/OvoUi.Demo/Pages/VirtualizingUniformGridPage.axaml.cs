using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OvoUi.Demo.Pages;

public partial class VirtualizingUniformGridPage : UserControl
{
    public VirtualizingUniformGridPage()
    {
        InitializeComponent();
        DataContext = new VirtualizingUniformGridDemoViewModel();
    }
}

public partial class VirtualizingUniformGridDemoViewModel : ObservableObject
{
    private static readonly IBrush[] Brushes =
    [
        Brush.Parse("#E57373"), Brush.Parse("#81C784"), Brush.Parse("#64B5F6"), Brush.Parse("#FFB74D"),
        Brush.Parse("#BA68C8"), Brush.Parse("#4DB6AC"), Brush.Parse("#FFF176"), Brush.Parse("#A1887F")
    ];

    public VirtualizingUniformGridDemoViewModel()
    {
        Items = Enumerable.Range(0, 100_000)
            .Select(i => new VirtualGridItem($"Item {i:N0}", Brushes[i % Brushes.Length]))
            .ToArray();
    }

    [ObservableProperty] private int _columns = 4;
    [ObservableProperty] private double _cacheLength = 0.5;
    [ObservableProperty] private double _itemHeight = 120;
    [ObservableProperty] private bool _uniformItemHeight = true;

    public IReadOnlyList<VirtualGridItem> Items { get; }

    public string SourceCode { get; } = """
                                        public VirtualizingUniformGridDemoViewModel()
                                        {
                                            Items = Enumerable.Range(0, 100_000)
                                                .Select(i => new VirtualGridItem($"Item {i:N0}"))
                                                .ToArray();
                                        }

                                        public IReadOnlyList<VirtualGridItem> Items { get; }
                                        public int Columns { get; set; } = 4;
                                        public double CacheLength { get; set; } = 0.5;
                                        public double ItemHeight { get; set; } = 120;
                                        """;
}

public sealed record VirtualGridItem(string Label, IBrush Background);
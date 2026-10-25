using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OvoUi.Demo.Pages;

public partial class EnumSelectorPage : UserControl
{
    public EnumSelectorPage()
    {
        InitializeComponent();
        DataContext = new EnumSelectorDemoViewModel();
    }
}

public partial class EnumSelectorDemoViewModel : ObservableObject
{
    public string SourceCode { get; } =
        """"
        public partial class EnumSelectorDemoViewModel : ObservableObject
        {
            [ObservableProperty] private Type? _selectedType;
            [ObservableProperty] private object? _value;

            public ObservableCollection<Type?> Types { get; set; } =
            [
                typeof(HorizontalAlignment),
                typeof(VerticalAlignment),
                typeof(Orientation),
                typeof(Dock),
                typeof(GridResizeDirection),
                typeof(DayOfWeek),
                typeof(FillMode),
                typeof(IterationType),
                typeof(BindingMode),
                typeof(BindingPriority),
                typeof(StandardCursorType),
                typeof(Key),
                typeof(KeyModifiers),
                typeof(RoutingStrategies),
                typeof(CustomEnum)
            ];
        }

        public enum CustomEnum
        {
            [Description("是")] Yes,
            [Description("否")] No,
        }
        """";

    [ObservableProperty] private Type? _selectedType;
    [ObservableProperty] private object? _value;

    public ObservableCollection<Type?> Types { get; set; } =
    [
        typeof(HorizontalAlignment),
        typeof(VerticalAlignment),
        typeof(Orientation),
        typeof(Dock),
        typeof(GridResizeDirection),
        typeof(DayOfWeek),
        typeof(FillMode),
        typeof(IterationType),
        typeof(BindingMode),
        typeof(BindingPriority),
        typeof(StandardCursorType),
        typeof(Key),
        typeof(KeyModifiers),
        typeof(RoutingStrategies),
        typeof(CustomEnum)
    ];
}

public enum CustomEnum
{
    [Description("是")] Yes,
    [Description("否")] No,
}
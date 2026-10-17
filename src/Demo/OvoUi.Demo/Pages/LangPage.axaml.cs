using Avalonia.Controls;
using Avalonia.Interactivity;
using OvoUi.Demo.Models;

namespace OvoUi.Demo.Pages;

public partial class LangPage : UserControl
{
    private readonly LangDemoViewModel _model = new();

    public LangPage()
    {
        InitializeComponent();
        DataContext = _model;
    }

    private void ApplyOverride(object? sender, RoutedEventArgs e) => _model.ApplyOverride();
    private void ClearOverrides(object? sender, RoutedEventArgs e) => _model.ClearOverrides();
}
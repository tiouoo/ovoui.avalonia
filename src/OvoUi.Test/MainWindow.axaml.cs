using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OvoUi.Common.Theme;

namespace OvoUi.Test;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AutoCompleteBox.ItemsSource = new List<string> { "Apple", "Banana", "Balalala", "Cherry", "Date" };
    }

    private void Dark(object? sender, RoutedEventArgs e)
    {
        ThemeManager.ToggleTheme(Themes.Dark);
    }

    private void Light(object? sender, RoutedEventArgs e)
    {
        ThemeManager.ToggleTheme(Themes.Light);
    }
}
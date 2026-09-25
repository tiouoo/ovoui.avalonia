using Avalonia.Controls;
using Avalonia.Interactivity;
using OvoUi.Common.Theme;

namespace OvoUi.Test;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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
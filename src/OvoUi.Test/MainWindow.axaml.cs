using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using OvoUi.Common.Theme;

namespace OvoUi.Test;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<string> StringItems { get; } =
    [
        "Button",
        "TextBox",
        "ComboBox",
        "DatePicker"
    ];

    public ObservableCollection<string> SelectedStringItems { get; } = [];
    
    
    
    
    
    
    
    
    
    
    private void Dark(object? sender, RoutedEventArgs e)
    {
        ThemeManager.ToggleTheme(Themes.Dark);
    }

    private void Light(object? sender, RoutedEventArgs e)
    {
        ThemeManager.ToggleTheme(Themes.Light);
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var sp = TopLevel.GetTopLevel(this).StorageProvider;
        if (sp is null) return;
        var result = await sp.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open File",
            FileTypeFilter =
            [
                FilePickerFileTypes.All,
                FilePickerFileTypes.TextPlain
            ],
            AllowMultiple = true,
        });
    }
}
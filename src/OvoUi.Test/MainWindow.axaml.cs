using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using OvoUi.Common.Language;
using OvoUi.Common.Theme;
using OvoUi.Controls;

namespace OvoUi.Test;

public partial class MainWindow : OvoWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }
    
    
    
    private void Dark(object? sender, RoutedEventArgs e)
    {
        ThemeManager.ToggleTheme(Themes.Dark);
    }

    private void Light(object? sender, RoutedEventArgs e)
    {
        ThemeManager.ToggleTheme(Themes.Light);
    }

    private void Chinese(object? sender, RoutedEventArgs e)
    {
        LangManager.SetLanguage(Languages.zh_cn);
    }

    private void English(object? sender, RoutedEventArgs e)
    {
        LangManager.SetLanguage(Languages.en_us);
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

    private void Button_OnClick1(object? sender, RoutedEventArgs e)
    {
        var a = new TitleBarExample();
        a.Show();
    }
}

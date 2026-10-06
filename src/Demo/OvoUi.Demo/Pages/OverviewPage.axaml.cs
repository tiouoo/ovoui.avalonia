using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace OvoUi.Demo.Pages;

public partial class OverviewPage : UserControl
{
    public OverviewPage()
    {
        InitializeComponent();
    }

    private async void OpenFile_OnClick(object? sender, RoutedEventArgs e)
    {
        var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storageProvider is null)
            return;

        await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            FileTypeFilter = [FilePickerFileTypes.All, FilePickerFileTypes.TextPlain],
            AllowMultiple = true
        });
    }
}

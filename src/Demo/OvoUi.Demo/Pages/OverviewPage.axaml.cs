using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using OvoUi.Common.Extension;

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

    private void OpenLink(object? sender, PointerPressedEventArgs e)
    {
        var url = (sender as Control).Tag as string;
        var launcher = this.GetTopLevel().Launcher;
        launcher.LaunchUriAsync(new Uri(url!));
    }
}

using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace OvoUi.Demo.Pages;

public partial class ManagedFileChooserPage : UserControl
{
    public ManagedFileChooserPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string SourceCode { get; } = """
                                        private async void OpenFile(object? sender, RoutedEventArgs e)
                                        {
                                            var files = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(
                                                new FilePickerOpenOptions { AllowMultiple = true, Title = "Open files" });
                                            PickerResult.Text = files.Count == 0
                                                ? "No file selected"
                                                : string.Join(Environment.NewLine, files.Select(file => file.Name));
                                        }

                                        private async void SelectFolder(object? sender, RoutedEventArgs e)
                                        {
                                            var folders = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFolderPickerAsync(
                                                new FolderPickerOpenOptions { AllowMultiple = false, Title = "Select folder" });
                                            PickerResult.Text = folders.Count == 0 ? "No folder selected" : folders[0].Name;
                                        }

                                        private async void SaveFile(object? sender, RoutedEventArgs e)
                                        {
                                            var file = await TopLevel.GetTopLevel(this)!.StorageProvider.SaveFilePickerAsync(
                                                new FilePickerSaveOptions { SuggestedFileName = "document.txt", Title = "Save file" });
                                            PickerResult.Text = file?.Name ?? "Save cancelled";
                                        }
                                        """;

    private async void OpenFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var files = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions { AllowMultiple = true, Title = "Open files" });
        PickerResult.Text = files.Count == 0
            ? "No file selected"
            : string.Join(Environment.NewLine, files.Select(file => file.Name));
    }

    private async void SelectFolder(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var folders = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { AllowMultiple = false, Title = "Select folder" });
        PickerResult.Text = folders.Count == 0 ? "No folder selected" : folders[0].Name;
    }

    private async void SaveFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var file = await TopLevel.GetTopLevel(this)!.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions { SuggestedFileName = "document.txt", Title = "Save file" });
        PickerResult.Text = file?.Name ?? "Save cancelled";
    }
}
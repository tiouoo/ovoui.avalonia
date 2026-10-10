using Avalonia.Controls;
using Avalonia.Interactivity;
namespace OvoUi.Demo.Pages;
public partial class ImageViewerPage : UserControl
{
 public ImageViewerPage() { InitializeComponent(); DataContext = this; }
 private void Fit_Click(object? sender, RoutedEventArgs e) => Viewer.FitToView();
 public string SourceCode { get; } = """private void Fit_Click(object? sender, RoutedEventArgs e) => Viewer.FitToView();""";
}


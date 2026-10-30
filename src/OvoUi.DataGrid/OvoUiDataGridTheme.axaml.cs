using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Avalonia.Styling;

[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi")]

namespace OvoUi;

/// <summary>Optional DataGrid theme for applications using the managed runtime.</summary>
public partial class OvoUiDataGridTheme : Styles
{
    public OvoUiDataGridTheme() => AvaloniaXamlLoader.Load(this);
}

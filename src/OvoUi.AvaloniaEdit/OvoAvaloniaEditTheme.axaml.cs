using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Avalonia.Styling;

[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.AvaloniaEdit")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.AvaloniaEdit.Controls")]

namespace OvoUi.AvaloniaEdit;

public partial class OvoAvaloniaEditTheme : Styles
{
    public OvoAvaloniaEditTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

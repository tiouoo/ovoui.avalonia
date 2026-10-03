using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Avalonia.Styling;

[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.Fonts")]

namespace OvoUi.Fonts;

public partial class OvoUiFont :  Styles
{
    public OvoUiFont()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
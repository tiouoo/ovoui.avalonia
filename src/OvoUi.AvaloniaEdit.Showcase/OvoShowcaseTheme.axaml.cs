using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Avalonia.Styling;

[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.AvaloniaEdit.Showcase")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.AvaloniaEdit.Showcase.Controls")]

namespace OvoUi.AvaloniaEdit.Showcase;

public partial class OvoShowcaseTheme : Styles
{
    public OvoShowcaseTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

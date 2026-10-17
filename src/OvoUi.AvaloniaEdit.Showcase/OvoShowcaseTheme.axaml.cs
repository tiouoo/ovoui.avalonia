using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Avalonia.Styling;
using OvoUi.Common.Language;

[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.AvaloniaEdit.Showcase")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.AvaloniaEdit.Showcase.Controls")]
[assembly: XmlnsPrefix("https://github.com/tiouoo/ovoui.avalonia", "ovo")]

namespace OvoUi.AvaloniaEdit.Showcase;

public partial class OvoShowcaseTheme : Styles
{
    public OvoShowcaseTheme()
    {
        LangManager.RegisterResourceProvider(ShowcaseLanguageResourceProvider.Instance);
        AvaloniaXamlLoader.Load(this);
    }
}

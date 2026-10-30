using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using OvoUi.Common.Extension;
using OvoUi.Common.Language;
using OvoUi.Common.Theme;

[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.Controls")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.Controls.Overlay")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.Common")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.Common.Helpers")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.Common.Classes")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.Common.Interfaces")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.Common.Converter")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.Theme.MarkupExtensions")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.Theme.Animations")]
[assembly: XmlnsDefinition("https://github.com/tiouoo/ovoui.avalonia", "OvoUi.Theme.Styling.Controls.ColorPicker")]
[assembly: XmlnsPrefix("https://github.com/tiouoo/ovoui.avalonia", "ovo")]

namespace OvoUi;

public partial class OvoUiDataGridTheme : Styles
{
    public OvoUiDataGridTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
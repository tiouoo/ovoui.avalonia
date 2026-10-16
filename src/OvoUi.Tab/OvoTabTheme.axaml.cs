using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using OvoUi.Common.Language;
using OvoUi.Tab.Internal;

namespace OvoUi.Tab;

public sealed partial class OvoTabTheme : Styles
{
    public OvoTabTheme()
    {
        LangManager.RegisterResourceProvider(TabLanguageResourceProvider.Instance);
        AvaloniaXamlLoader.Load(this);
    }
}

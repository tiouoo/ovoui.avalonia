using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace OvoUi.Tab;

public sealed partial class OvoTabTheme : Styles
{
    public OvoTabTheme() => AvaloniaXamlLoader.Load(this);
}

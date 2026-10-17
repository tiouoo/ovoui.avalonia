using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace OvoUi.SmoothScroll;

/// <summary>Styles only the optional <see cref="Controls.OvoScrollView"/> control.</summary>
public sealed partial class OvoScrollViewTheme : Styles
{
    public OvoScrollViewTheme() => AvaloniaXamlLoader.Load(this);
}
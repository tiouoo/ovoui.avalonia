using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace OvoUi.SmoothScroll;

/// <summary>Enables smooth scrolling for standard ScrollViewer controls within its resource scope.</summary>
public sealed partial class OvoScrollViewerTheme : Styles
{
    public OvoScrollViewerTheme() => AvaloniaXamlLoader.Load(this);
}
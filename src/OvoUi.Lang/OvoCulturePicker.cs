using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace OvoUi.Lang;

/// <summary>
/// Provides the default styles for <see cref="CulturePicker" />.
/// </summary>
public sealed partial class OvoCulturePicker : Styles
{
    public OvoCulturePicker()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

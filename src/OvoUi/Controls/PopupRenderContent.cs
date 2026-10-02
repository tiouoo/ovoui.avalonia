using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace OvoUi.Controls;

public sealed class PopupRenderContent : ContentControl
{
    public static readonly StyledProperty<bool> ExpandUpProperty =
        AvaloniaProperty.Register<PopupRenderContent, bool>(nameof(ExpandUp));

    public bool ExpandUp
    {
        get => GetValue(ExpandUpProperty);
        set => SetValue(ExpandUpProperty, value);
    }
}

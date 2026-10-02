using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace OvoUi.Controls;

public sealed class PopupContentExpander : ContentExpander
{
    public static readonly StyledProperty<bool> ExpandUpProperty =
        AvaloniaProperty.Register<PopupContentExpander, bool>(nameof(ExpandUp));

    public static readonly AttachedProperty<bool> DisableAnimationProperty =
        AvaloniaProperty.RegisterAttached<PopupContentExpander, Control, bool>("DisableAnimation");

    public bool ExpandUp
    {
        get => GetValue(ExpandUpProperty);
        set => SetValue(ExpandUpProperty, value);
    }

    public static bool GetDisableAnimation(Control element)
    {
        return element.GetValue(DisableAnimationProperty);
    }

    public static void SetDisableAnimation(Control element, bool value)
    {
        element.SetValue(DisableAnimationProperty, value);
    }

    public PopupContentExpander()
    {
        Orientation = Avalonia.Layout.Orientation.Vertical;
        Multiplier = 1;
        Opacity = 1;
    }
}

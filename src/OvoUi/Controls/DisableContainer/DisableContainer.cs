using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Metadata;

namespace OvoUi.Controls;

public class DisableContainer : TemplatedControl
{
    public static readonly StyledProperty<InputElement?> ContentProperty =
        AvaloniaProperty.Register<DisableContainer, InputElement?>(
            nameof(Content));

    public static readonly StyledProperty<object?> DisabledTipProperty =
        AvaloniaProperty.Register<DisableContainer, object?>(
            nameof(DisabledTip));

    [Content]
    public InputElement? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public object? DisabledTip
    {
        get => GetValue(DisabledTipProperty);
        set => SetValue(DisabledTipProperty, value);
    }
}
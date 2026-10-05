using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace OvoUi.Controls;

public class GroupBorder : TemplatedControl
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<GroupBorder, object?>(nameof(Header));

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<GroupBorder, object?>(nameof(Content));

    public static readonly StyledProperty<bool> IsHeaderSelectableProperty =
        AvaloniaProperty.Register<GroupBorder, bool>(nameof(IsHeaderSelectable));

    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public bool IsHeaderSelectable
    {
        get => GetValue(IsHeaderSelectableProperty);
        set => SetValue(IsHeaderSelectableProperty, value);
    }
}
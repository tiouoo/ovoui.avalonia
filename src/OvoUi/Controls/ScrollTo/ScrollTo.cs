using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using OvoUi.Common;
using OvoUi.Common.Enums;

namespace OvoUi.Controls;

public class ScrollTo
{
    public static readonly AttachedProperty<Position?> DirectionProperty =
        AvaloniaProperty.RegisterAttached<ScrollTo, Control, Position?>("Direction");

    public static readonly AttachedProperty<ControlTheme?> ButtonThemeProperty =
        AvaloniaProperty.RegisterAttached<ScrollTo, Control, ControlTheme?>("ButtonTheme");

    static ScrollTo()
    {
        DirectionProperty.Changed.AddClassHandler<Control, Position?>(OnDirectionChanged);
        ButtonThemeProperty.Changed.AddClassHandler<Control, ControlTheme?>(OnButtonThemeChanged);
    }

    public static void SetDirection(Control obj, Position value) => obj.SetValue(DirectionProperty, value);
    public static Position? GetDirection(Control obj) => obj.GetValue(DirectionProperty);

    public static void SetButtonTheme(Control obj, ControlTheme? value) => obj.SetValue(ButtonThemeProperty, value);
    public static ControlTheme? GetButtonTheme(Control obj) => obj.GetValue(ButtonThemeProperty);

    private static void OnButtonThemeChanged(Control arg1, AvaloniaPropertyChangedEventArgs<ControlTheme?> arg2)
    {
        var button = EnsureButtonInAdorner(arg1);
        button.SetCurrentValue(StyledElement.ThemeProperty, arg2.NewValue.Value);
    }

    private static void OnDirectionChanged(Control control, AvaloniaPropertyChangedEventArgs<Position?> args)
    {
        if (args.NewValue.Value is null) return;
        var button = EnsureButtonInAdorner(control);
        button.SetCurrentValue(ScrollToButton.DirectionProperty, args.NewValue.Value);
    }

    private static ScrollToButton EnsureButtonInAdorner(Control control)
    {
        var adorner = AdornerLayer.GetAdorner(control);
        if (adorner is not ScrollToButton button)
        {
            button = new ScrollToButton();
            AdornerLayer.SetAdorner(control, button);
        }

        button.SetCurrentValue(ScrollToButton.TargetProperty, control);
        button.SetCurrentValue(ScrollToButton.DirectionProperty, GetDirection(control));
        if (GetButtonTheme(control) is { } theme)
        {
            button.SetCurrentValue(StyledElement.ThemeProperty, theme);
        }

        return button;
    }
}
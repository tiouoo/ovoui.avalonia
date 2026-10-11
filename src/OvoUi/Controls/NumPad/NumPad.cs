using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using OvoUi.Common.Enums;
using OvoUi.Common.Helpers;

namespace OvoUi.Controls;

public class NumPad : TemplatedControl
{
    private const double EstimatedWidth = 246;
    private const double EstimatedHeight = 304;

    public static readonly StyledProperty<InputElement?> TargetProperty =
        AvaloniaProperty.Register<NumPad, InputElement?>(nameof(Target));

    public static readonly StyledProperty<bool> NumModeProperty =
        AvaloniaProperty.Register<NumPad, bool>(nameof(NumMode), defaultValue: true);

    public static readonly AttachedProperty<bool> AttachProperty =
        AvaloniaProperty.RegisterAttached<NumPad, InputElement, bool>("Attach");

    private static readonly Dictionary<Key, string> KeyInputMapping = new()
    {
        [Key.NumPad0] = "0",
        [Key.NumPad1] = "1",
        [Key.NumPad2] = "2",
        [Key.NumPad3] = "3",
        [Key.NumPad4] = "4",
        [Key.NumPad5] = "5",
        [Key.NumPad6] = "6",
        [Key.NumPad7] = "7",
        [Key.NumPad8] = "8",
        [Key.NumPad9] = "9",
        [Key.Add] = "+",
        [Key.Subtract] = "-",
        [Key.Multiply] = "*",
        [Key.Divide] = "/",
        [Key.Decimal] = ".",
    };

    private TextBox? _targetInnerText;

    static NumPad()
    {
        AttachProperty.Changed.AddClassHandler<InputElement, bool>(OnAttachChanged);
    }

    public InputElement? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public bool NumMode
    {
        get => GetValue(NumModeProperty);
        set => SetValue(NumModeProperty, value);
    }

    public static void SetAttach(InputElement target, bool value) => target.SetValue(AttachProperty, value);

    public static bool GetAttach(InputElement target) => target.GetValue(AttachProperty);

    private static void OnAttachChanged(InputElement input, AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.NewValue.Value)
            GotFocusEvent.AddHandler(OnTargetGotFocus, input);
        else
            GotFocusEvent.RemoveHandler(OnTargetGotFocus, input);
    }

    private static void OnTargetGotFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not InputElement element)
            return;

        var existing = OverlayDialog.Recall<NumPad>(null);
        if (existing is not null)
        {
            if (existing.Target is IPv4Box previousIpv4Box)
                previousIpv4Box.IsTargetByNumPad = false;

            existing.Target = element;
            existing._targetInnerText = FindTextBoxInTarget(element);

            if (element is IPv4Box ipv4Box)
                ipv4Box.IsTargetByNumPad = true;

            return;
        }

        var numPad = new NumPad
        {
            Target = element,
            _targetInnerText = FindTextBoxInTarget(element),
        };
        if (element is IPv4Box targetIpv4Box)
            targetIpv4Box.IsTargetByNumPad = true;

        var options = BuildPositionedOptions(element as Control) ?? new OverlayDialogOptions
        {
            Buttons = DialogButton.None,
            CanLightDismiss = true,
            OnDialogControlClosed = new EventHandler<RoutedEventArgs>((_, _) => numPad.OnClosed()),
        };
        options.OnDialogControlClosed = new EventHandler<RoutedEventArgs>((_, _) => numPad.OnClosed());
        OverlayDialog.ShowStandard(numPad, new object(), options: options);
    }

    public void ProcessClick(object parameter)
    {
        if (Target is null || parameter is not NumPadButton button)
            return;

        var key = (button.NumMode ? button.NumKey : button.FunctionKey) ?? Key.None;
        var realTarget = _targetInnerText ?? Target;
        if (KeyInputMapping.TryGetValue(key, out var text))
        {
            realTarget.RaiseEvent(new TextInputEventArgs
            {
                Source = this,
                RoutedEvent = TextInputEvent,
                Text = text,
            });
        }
        else
        {
            realTarget.RaiseEvent(new KeyEventArgs
            {
                Source = this,
                RoutedEvent = KeyDownEvent,
                Key = key,
            });
        }
    }

    private void OnClosed()
    {
        if (Target is IPv4Box ipv4Box)
            ipv4Box.IsTargetByNumPad = false;
        Target?.Focus();
    }

    private static TextBox? FindTextBoxInTarget(InputElement target)
    {
        if (target is TextBox textBox)
            return textBox;

        if (target is TemplatedControl { IsInitialized: true } templatedControl &&
            templatedControl.GetVisualDescendants().OfType<TextBox>().FirstOrDefault() is { } templateTextBox)
            return templateTextBox;

        return target is ILogical logical
            ? logical.GetLogicalDescendants().OfType<TextBox>().FirstOrDefault()
            : null;
    }

    private static OverlayDialogOptions? BuildPositionedOptions(Control? target)
    {
        var topLevel = TopLevel.GetTopLevel(target);
        if (topLevel is null || target is null)
            return null;

        var rect = GetTargetRect(target, topLevel);
        var horizontalOffset = rect.Left;
        if (horizontalOffset + EstimatedWidth > topLevel.Bounds.Width)
            horizontalOffset = Math.Max(0, rect.Right - EstimatedWidth);

        var showBelow = topLevel.Bounds.Height - rect.Bottom >= EstimatedHeight;
        return new OverlayDialogOptions
        {
            Buttons = DialogButton.None,
            CanLightDismiss = true,
            CanDragMove = false,
            HorizontalAnchor = HorizontalPosition.Left,
            IsCloseButtonVisible = false,
            StyleClass = "num-pad",
            VerticalAnchor = showBelow ? VerticalPosition.Top : VerticalPosition.Bottom,
            HorizontalOffset = horizontalOffset,
            VerticalOffset = showBelow ? rect.Bottom : topLevel.Bounds.Height - rect.Top,
        };
    }

    private static Rect GetTargetRect(Control target, TopLevel topLevel)
    {
        var point = target.TranslatePoint(default, topLevel);
        return point.HasValue ? new Rect(point.Value, target.Bounds.Size) : default;
    }
}

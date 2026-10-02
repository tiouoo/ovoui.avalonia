using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace OvoUi.Controls;

public sealed class PopupContentExpander : ContentExpander
{
    private static readonly TimeSpan ExpandDuration = TimeSpan.FromMilliseconds(360);
    private static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(210);
    private int _animationVersion;

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
        ClipToBounds = true;
        PreserveDesiredSize = true;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (GetDisableAnimation(this))
        {
            Multiplier = 1;
            Opacity = 1;
            Transitions = null;
            return;
        }

        var animationVersion = ++_animationVersion;
        Dispatcher.UIThread.Post(() =>
        {
            if (animationVersion != _animationVersion)
                return;

            Multiplier = 0;
            Opacity = 0;
            var transitions = new Transitions
            {
                new DoubleTransition
                {
                    Property = OpacityProperty,
                    Duration = FadeDuration,
                    Easing = new Avalonia.Animation.Easings.QuadraticEaseOut()
                }
            };

            if (!ExpandUp)
            {
                transitions.Add(new DoubleTransition
                {
                    Property = MultiplierProperty,
                    Duration = ExpandDuration,
                    Easing = new Avalonia.Animation.Easings.ExponentialEaseOut()
                });
            }

            Transitions = transitions;
            Multiplier = 1;
            Opacity = 1;
        }, DispatcherPriority.Render);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        if (PreserveDesiredSize && Multiplier < 1)
        {
            Clip = new Avalonia.Media.RectangleGeometry(
                ExpandUp
                    ? new Rect(0, finalSize.Height * (1 - Multiplier), finalSize.Width, finalSize.Height * Multiplier)
                    : new Rect(0, 0, finalSize.Width, finalSize.Height * Multiplier));
        }
        else
        {
            Clip = null;
        }

        return result;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _animationVersion++;
        base.OnDetachedFromVisualTree(e);
    }
}

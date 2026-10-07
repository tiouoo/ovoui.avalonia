using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using OvoUi.Theme.Animations;

namespace OvoUi.AvaloniaEdit.Showcase.Controls;

/// <summary>
/// Moves pages horizontally while fading them, reversing direction for backward navigation.
/// </summary>
public sealed class SlideFadePageTransition : IPageTransition
{
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(160);

    public double Distance { get; set; } = 28d;

    /// <summary>
    /// Makes the new page enter from the left while the old page exits to the left.
    /// </summary>
    public bool EnterFromLeft { get; set; }

    public Easing Easing { get; set; } = new SukiEaseOut();

    public async Task Start(
        Visual? from,
        Visual? to,
        bool forward,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        var tasks = new List<Task>(2);
        var direction = forward ? 1d : -1d;

        if (from is not null)
            tasks.Add(CreateAnimation(0d, -Distance * direction, 1d, 0d).RunAsync(from, cancellationToken));

        if (to is not null)
        {
            to.IsVisible = true;
            var enterFromX = EnterFromLeft ? -Distance : Distance * direction;
            tasks.Add(CreateAnimation(enterFromX, 0d, 0d, 1d).RunAsync(to, cancellationToken));
        }

        await Task.WhenAll(tasks);

        if (cancellationToken.IsCancellationRequested)
            return;

        if (from is not null)
        {
            from.IsVisible = false;
            Reset(from);
        }

        if (to is not null)
            Reset(to);
    }

    private Animation CreateAnimation(double fromX, double toX, double fromOpacity, double toOpacity)
    {
        return new Animation
        {
            Duration = Duration,
            Easing = Easing,
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, fromX),
                        new Setter(Visual.OpacityProperty, fromOpacity)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, toX),
                        new Setter(Visual.OpacityProperty, toOpacity)
                    }
                }
            }
        };
    }

    private static void Reset(Visual visual)
    {
        visual.RenderTransform = null;
        visual.Opacity = 1d;
    }
}

using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using OvoUi.Common.Helpers;

namespace OvoUi.Theme.Animations;

public class NavMenuSizeAnimations : ResourceDictionary
{
    public const string NavMenuWidthAnimationGeneratorKey = "NavMenuWidthAnimationGenerator";

    private readonly SizeAnimationHelperAnimationGeneratorDelegate _navMenuWidthAnimationGenerator =
        (_, oldDesiredSize, newDesiredSize) =>
        {
            var startScale = newDesiredSize.Width > 0
                ? oldDesiredSize.Width / newDesiredSize.Width
                : 1;

            if (!double.IsFinite(startScale) || startScale <= 0)
                startScale = 1;

            return new Animation
            {
                Duration = TimeSpan.FromMilliseconds(300),
                Easing = new CubicEaseInOut(),
                FillMode = FillMode.None,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0.0),
                        Setters =
                        {
                            new Setter(ScaleTransform.ScaleXProperty, startScale)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1.0),
                        Setters =
                        {
                            new Setter(ScaleTransform.ScaleXProperty, 1d)
                        }
                    }
                }
            };
        };

    public NavMenuSizeAnimations()
    {
        Add(NavMenuWidthAnimationGeneratorKey, _navMenuWidthAnimationGenerator);
    }
}

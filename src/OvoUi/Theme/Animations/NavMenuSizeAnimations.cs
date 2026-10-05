using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;
using OvoUi.Common.Helpers;

namespace OvoUi.Theme.Animations;

public class NavMenuSizeAnimations : ResourceDictionary
{
    public const string NavMenuWidthAnimationGeneratorKey = "NavMenuWidthAnimationGenerator";

    private readonly SizeAnimationHelperAnimationGeneratorDelegate _navMenuWidthAnimationGenerator =
        (_, oldDesiredSize, newDesiredSize) =>
        {
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
                            new Setter(Layoutable.WidthProperty, oldDesiredSize.Width)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1.0),
                        Setters =
                        {
                            new Setter(Layoutable.WidthProperty, newDesiredSize.Width)
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

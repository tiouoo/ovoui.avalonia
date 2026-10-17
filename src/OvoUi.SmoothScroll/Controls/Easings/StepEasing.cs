using Avalonia.Animation.Easings;

namespace OvoUi.SmoothScroll.Controls.Easings;


public class StepEasing : Easing
{
    public override double Ease(double progress)
    {
        return progress < 0.5 ? 0 : 1;
    }
}

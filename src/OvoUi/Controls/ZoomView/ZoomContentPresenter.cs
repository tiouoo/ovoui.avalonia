using Avalonia;
using Avalonia.Controls.Presenters;

namespace OvoUi.Controls;

/// <summary>Preserves natural content size and origin inside a zoom viewport.</summary>
public class ZoomContentPresenter : ContentPresenter
{
    protected override Size MeasureOverride(Size availableSize) => base.MeasureOverride(Size.Infinity);

    protected override Size ArrangeOverride(Size finalSize)
    {
        Child?.Arrange(new Rect(Child.DesiredSize));
        return finalSize;
    }
}

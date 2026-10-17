using Avalonia;

namespace OvoUi.Controls;

internal sealed class ElasticStretchCalculator
{
    private static readonly SaturatingFunction AxialStretch = new(16, -618);
    private static readonly SaturatingFunction CrossAxisShrink = new(0.382, -618);
    private static readonly SaturatingFunction Offset = new(24, -618);

    public Size OriginalSize { get; set; }
    public Vector DragDelta { get; set; }
    public double StretchX { get; private set; }
    public double StretchY { get; private set; }
    public double OffsetX { get; private set; }
    public double OffsetY { get; private set; }

    public void Calculate()
    {
        var deltaX = Math.Abs(DragDelta.X);
        var deltaY = Math.Abs(DragDelta.Y);

        StretchX = AxialStretch.Calculate(deltaX) - CrossAxisShrink.Calculate(deltaY) * OriginalSize.Width;
        StretchY = AxialStretch.Calculate(deltaY) - CrossAxisShrink.Calculate(deltaX) * OriginalSize.Height;
        OffsetX = Offset.Calculate(deltaX);
        OffsetY = Offset.Calculate(deltaY);
    }

    private sealed class SaturatingFunction(double limit, double growth)
    {
        public double Calculate(double value) => value == 0 ? 0 : limit * value / (value - growth);
    }
}
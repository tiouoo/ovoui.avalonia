using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace OvoUi.Controls;

public class DiffOverviewMarkerLayer : Control
{
    public static readonly StyledProperty<IReadOnlyList<DiffMarker>?> MarkersProperty =
        AvaloniaProperty.Register<DiffOverviewMarkerLayer, IReadOnlyList<DiffMarker>?>(nameof(Markers));
    public static readonly StyledProperty<IBrush?> AddedBrushProperty =
        AvaloniaProperty.Register<DiffOverviewMarkerLayer, IBrush?>(nameof(AddedBrush), Brushes.Green);
    public static readonly StyledProperty<IBrush?> RemovedBrushProperty =
        AvaloniaProperty.Register<DiffOverviewMarkerLayer, IBrush?>(nameof(RemovedBrush), Brushes.Red);
    public static readonly StyledProperty<IBrush?> ModifiedBrushProperty =
        AvaloniaProperty.Register<DiffOverviewMarkerLayer, IBrush?>(nameof(ModifiedBrush), Brushes.DodgerBlue);

    static DiffOverviewMarkerLayer() => AffectsRender<DiffOverviewMarkerLayer>(
        MarkersProperty, AddedBrushProperty, RemovedBrushProperty, ModifiedBrushProperty);

    public IReadOnlyList<DiffMarker>? Markers
    {
        get => GetValue(MarkersProperty);
        set => SetValue(MarkersProperty, value);
    }

    public IBrush? AddedBrush
    {
        get => GetValue(AddedBrushProperty);
        set => SetValue(AddedBrushProperty, value);
    }

    public IBrush? RemovedBrush
    {
        get => GetValue(RemovedBrushProperty);
        set => SetValue(RemovedBrushProperty, value);
    }

    public IBrush? ModifiedBrush
    {
        get => GetValue(ModifiedBrushProperty);
        set => SetValue(ModifiedBrushProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Bounds.Width <= 0 || Bounds.Height <= 0 || Markers is not { Count: > 0 })
            return;

        var width = Math.Min(5, Bounds.Width);
        foreach (var marker in Markers)
        {
            var brush = marker.Kind switch
            {
                DiffLineKind.Added => AddedBrush,
                DiffLineKind.Removed => RemovedBrush,
                _ => ModifiedBrush
            };
            var y = marker.YRatio * Bounds.Height;
            var height = Math.Min(Bounds.Height - y, Math.Max(1, marker.HeightRatio * Bounds.Height));
            context.DrawRectangle(brush, null, new Rect((Bounds.Width - width) / 2, y, width, height));
        }
    }
}

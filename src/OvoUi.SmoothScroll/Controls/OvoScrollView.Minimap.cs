using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace OvoUi.SmoothScroll.Controls;

[TemplatePart(PART_Minimap, typeof(ZoomMinimap))]
public partial class OvoScrollView
{
    public const string PART_Minimap = nameof(PART_Minimap);
    private ZoomMinimap? _minimap;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_minimap is not null) _minimap.ViewportChanged -= OnMinimapViewportChanged;
        base.OnApplyTemplate(e);
        _minimap = e.NameScope.Find<ZoomMinimap>(PART_Minimap);
        if (_minimap is not null) _minimap.ViewportChanged += OnMinimapViewportChanged;
        UpdateMinimap();
    }

    partial void OnPropertyChangedOverride(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == OffsetProperty || change.Property == ExtentProperty ||
            change.Property == ViewportProperty || change.Property == ZoomFactorProperty ||
            change.Property == IsMinimapVisibleProperty)
            UpdateMinimap();
    }

    private void UpdateMinimap()
    {
        if (_minimap is null || !IsMinimapVisible) return;
        var scale = ZoomFactor;
        _minimap.SetCurrentValue(ZoomMinimap.ContentSizeProperty,
            new Size(Extent.Width / scale, Extent.Height / scale));
        _minimap.SetCurrentValue(ZoomMinimap.ViewportRectProperty,
            new Rect(Offset.X / scale, Offset.Y / scale, Viewport.Width / scale, Viewport.Height / scale));
    }

    private void OnMinimapViewportChanged(object? sender, Point position) =>
        SetCurrentValue(OffsetProperty, new Vector(position.X * ZoomFactor, position.Y * ZoomFactor));
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

#if OVO_SMOOTH_SCROLL
namespace OvoUi.SmoothScroll.Controls;
#else
namespace OvoUi.Controls;
#endif

public class ZoomMinimap : Control
{
    public static readonly StyledProperty<Size> ContentSizeProperty =
        AvaloniaProperty.Register<ZoomMinimap, Size>(nameof(ContentSize));
    public static readonly StyledProperty<Rect> ViewportRectProperty =
        AvaloniaProperty.Register<ZoomMinimap, Rect>(nameof(ViewportRect));
    public static readonly StyledProperty<bool> IsNavigationEnabledProperty =
        AvaloniaProperty.Register<ZoomMinimap, bool>(nameof(IsNavigationEnabled), true);
    public static readonly StyledProperty<bool> IsDragEnabledProperty =
        AvaloniaProperty.Register<ZoomMinimap, bool>(nameof(IsDragEnabled), true);
    public static readonly StyledProperty<Vector> OffsetProperty =
        AvaloniaProperty.Register<ZoomMinimap, Vector>(nameof(Offset));
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<ZoomMinimap, IBrush?>(nameof(Background), Brush.Parse("#4F4C4C4C"));
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<ZoomMinimap, IBrush?>(nameof(BorderBrush), Brushes.Transparent);
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<ZoomMinimap, CornerRadius>(nameof(CornerRadius), new CornerRadius(8));

    public IBrush? Background { get => GetValue(BackgroundProperty); set => SetValue(BackgroundProperty, value); }
    public IBrush? BorderBrush { get => GetValue(BorderBrushProperty); set => SetValue(BorderBrushProperty, value); }
    public CornerRadius CornerRadius { get => GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }

    public Vector Offset { get => GetValue(OffsetProperty); set => SetValue(OffsetProperty, value); }

    public Size ContentSize { get => GetValue(ContentSizeProperty); set => SetValue(ContentSizeProperty, value); }
    public Rect ViewportRect { get => GetValue(ViewportRectProperty); set => SetValue(ViewportRectProperty, value); }
    public bool IsNavigationEnabled { get => GetValue(IsNavigationEnabledProperty); set => SetValue(IsNavigationEnabledProperty, value); }
    public bool IsDragEnabled { get => GetValue(IsDragEnabledProperty); set => SetValue(IsDragEnabledProperty, value); }

    private IPointer? _pointer;
    private Vector _grabOffset;

    public event EventHandler<Point>? ViewportChanged;

    static ZoomMinimap() => AffectsRender<ZoomMinimap>(ContentSizeProperty, ViewportRectProperty,
        BackgroundProperty, BorderBrushProperty, CornerRadiusProperty);

    private (Rect Area, double Scale) GetContentArea()
    {
        var size = ContentSize;
        if (!(size.Width > 0 && size.Height > 0) || !double.IsFinite(size.Width) || !double.IsFinite(size.Height))
            return default;
        var scale = Math.Min(Bounds.Width / size.Width, Bounds.Height / size.Height);
        return (new Rect((Bounds.Width - size.Width * scale) / 2,
            (Bounds.Height - size.Height * scale) / 2, size.Width * scale, size.Height * scale), scale);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawRectangle(Brushes.Transparent, null, new Rect(Bounds.Size));
        var (area, scale) = GetContentArea();
        if (scale <= 0) return;
        var surface = new RoundedRect(area, CornerRadius);
        context.DrawRectangle(Background, null, surface);
        var viewport = ViewportRect;
        var thumb = new Rect(area.X + viewport.X * scale, area.Y + viewport.Y * scale,
            viewport.Width * scale, viewport.Height * scale).Intersect(area);
        if (thumb.Width > 0 && thumb.Height > 0)
        {
            using (context.PushClip(surface))
                context.DrawRectangle(new SolidColorBrush(Color.Parse("#66549EFF")),
                    new Pen(new SolidColorBrush(Color.Parse("#FF71B4FF")), 1.5), new RoundedRect(thumb, CornerRadius));
        }
        context.DrawRectangle(null, new Pen(BorderBrush, 0), surface);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!IsNavigationEnabled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        var (area, scale) = GetContentArea();
        if (scale <= 0) return;
        var position = e.GetPosition(this);
        var contentPoint = new Point((position.X - area.X) / scale, (position.Y - area.Y) / scale);
        // Preserve the grab position when dragging the existing viewport rectangle.
        _grabOffset = ViewportRect.Contains(contentPoint)
            ? contentPoint - ViewportRect.Position
            : new Vector(ViewportRect.Width / 2, ViewportRect.Height / 2);
        RequestViewport(position);
        if (IsDragEnabled)
        {
            _pointer = e.Pointer;
            e.Pointer.Capture(this);
        }
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_pointer != e.Pointer) return;
        RequestViewport(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_pointer != e.Pointer) return;
        ReleasePointer();
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _pointer = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OffsetProperty)
            RenderTransform = new TranslateTransform(Offset.X, Offset.Y);
        if ((change.Property == IsDragEnabledProperty && !IsDragEnabled) ||
            (change.Property == IsNavigationEnabledProperty && !IsNavigationEnabled) ||
            (change.Property == IsVisibleProperty && !IsVisible))
            ReleasePointer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleasePointer();
        base.OnDetachedFromVisualTree(e);
    }

    private void ReleasePointer()
    {
        var pointer = _pointer;
        _pointer = null;
        pointer?.Capture(null);
    }

    private void RequestViewport(Point position)
    {
        var (area, scale) = GetContentArea();
        if (scale <= 0 || !IsNavigationEnabled) return;
        ViewportChanged?.Invoke(this, new Point(
            Math.Clamp((position.X - area.X) / scale - _grabOffset.X, 0, Math.Max(0, ContentSize.Width - ViewportRect.Width)),
            Math.Clamp((position.Y - area.Y) / scale - _grabOffset.Y, 0, Math.Max(0, ContentSize.Height - ViewportRect.Height))));
    }
}

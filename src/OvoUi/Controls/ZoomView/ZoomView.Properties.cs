using Avalonia;
using Avalonia.Layout;
using Avalonia.Input;

namespace OvoUi.Controls;

public partial class ZoomView
{
    /// <summary>Modifier keys required for mouse-wheel zoom. Use None for wheel-only zoom.</summary>
    public static readonly StyledProperty<KeyModifiers> WheelZoomModifiersProperty =
        AvaloniaProperty.Register<ZoomView, KeyModifiers>(nameof(WheelZoomModifiers), KeyModifiers.Control);

    public KeyModifiers WheelZoomModifiers
    {
        get => GetValue(WheelZoomModifiersProperty);
        set => SetValue(WheelZoomModifiersProperty, value);
    }

    public static readonly StyledProperty<MouseButton> PanButtonProperty =
        AvaloniaProperty.Register<ZoomView, MouseButton>(nameof(PanButton), MouseButton.Middle,
            validate: value => value is MouseButton.Left or MouseButton.Right or MouseButton.Middle);

    public MouseButton PanButton
    {
        get => GetValue(PanButtonProperty);
        set => SetValue(PanButtonProperty, value);
    }

    public static readonly StyledProperty<Vector> MinimapOffsetProperty =
        AvaloniaProperty.Register<ZoomView, Vector>(nameof(MinimapOffset),
            validate: value => double.IsFinite(value.X) && double.IsFinite(value.Y));

    public Vector MinimapOffset
    {
        get => GetValue(MinimapOffsetProperty);
        set => SetValue(MinimapOffsetProperty, value);
    }

    public static readonly StyledProperty<bool> IsMinimapVisibleProperty =
        AvaloniaProperty.Register<ZoomView, bool>(nameof(IsMinimapVisible), true);

    public bool IsMinimapVisible
    {
        get => GetValue(IsMinimapVisibleProperty);
        set => SetValue(IsMinimapVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> IsMinimapDragEnabledProperty =
        AvaloniaProperty.Register<ZoomView, bool>(nameof(IsMinimapDragEnabled), true);

    public bool IsMinimapDragEnabled
    {
        get => GetValue(IsMinimapDragEnabledProperty);
        set => SetValue(IsMinimapDragEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> IsMinimapNavigationEnabledProperty =
        AvaloniaProperty.Register<ZoomView, bool>(nameof(IsMinimapNavigationEnabled), true);

    public bool IsMinimapNavigationEnabled
    {
        get => GetValue(IsMinimapNavigationEnabledProperty);
        set => SetValue(IsMinimapNavigationEnabledProperty, value);
    }

    public static readonly StyledProperty<double> MinimapWidthProperty =
        AvaloniaProperty.Register<ZoomView, double>(nameof(MinimapWidth), 180, validate: value => double.IsFinite(value) && value > 0);

    public double MinimapWidth
    {
        get => GetValue(MinimapWidthProperty);
        set => SetValue(MinimapWidthProperty, value);
    }

    public static readonly StyledProperty<double> MinimapHeightProperty =
        AvaloniaProperty.Register<ZoomView, double>(nameof(MinimapHeight), 120, validate: value => double.IsFinite(value) && value > 0);

    public double MinimapHeight
    {
        get => GetValue(MinimapHeightProperty);
        set => SetValue(MinimapHeightProperty, value);
    }

    public static readonly StyledProperty<Thickness> MinimapMarginProperty =
        AvaloniaProperty.Register<ZoomView, Thickness>(nameof(MinimapMargin), new Thickness(8));

    public Thickness MinimapMargin
    {
        get => GetValue(MinimapMarginProperty);
        set => SetValue(MinimapMarginProperty, value);
    }

    public static readonly StyledProperty<HorizontalAlignment> MinimapHorizontalAlignmentProperty =
        AvaloniaProperty.Register<ZoomView, HorizontalAlignment>(nameof(MinimapHorizontalAlignment), HorizontalAlignment.Right);

    public HorizontalAlignment MinimapHorizontalAlignment
    {
        get => GetValue(MinimapHorizontalAlignmentProperty);
        set => SetValue(MinimapHorizontalAlignmentProperty, value);
    }

    public static readonly StyledProperty<VerticalAlignment> MinimapVerticalAlignmentProperty =
        AvaloniaProperty.Register<ZoomView, VerticalAlignment>(nameof(MinimapVerticalAlignment), VerticalAlignment.Bottom);

    public VerticalAlignment MinimapVerticalAlignment
    {
        get => GetValue(MinimapVerticalAlignmentProperty);
        set => SetValue(MinimapVerticalAlignmentProperty, value);
    }

    public static readonly StyledProperty<bool> IsPanEnabledProperty =
        AvaloniaProperty.Register<ZoomView, bool>(nameof(IsPanEnabled), true);

    public bool IsPanEnabled
    {
        get => GetValue(IsPanEnabledProperty);
        set => SetValue(IsPanEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> IsZoomEnabledProperty =
        AvaloniaProperty.Register<ZoomView, bool>(nameof(IsZoomEnabled), true);

    public bool IsZoomEnabled
    {
        get => GetValue(IsZoomEnabledProperty);
        set => SetValue(IsZoomEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> ShowScrollBarsProperty =
        AvaloniaProperty.Register<ZoomView, bool>(nameof(ShowScrollBars), true);

    public bool ShowScrollBars
    {
        get => GetValue(ShowScrollBarsProperty);
        set => SetValue(ShowScrollBarsProperty, value);
    }

}

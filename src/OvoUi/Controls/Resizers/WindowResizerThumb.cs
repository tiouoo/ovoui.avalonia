using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace OvoUi.Controls;

public class WindowResizerThumb : Thumb
{
    public static readonly StyledProperty<ResizeDirection> ResizeDirectionProperty =
        AvaloniaProperty.Register<WindowResizerThumb, ResizeDirection>(
            nameof(ResizeDirection));

    private Window? _window;

    public ResizeDirection ResizeDirection
    {
        get => GetValue(ResizeDirectionProperty);
        set => SetValue(ResizeDirectionProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _window = TopLevel.GetTopLevel(this) as Window;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (_window is null || !_window.CanResize) return;
        // TODO: Support touch screen resizing but we don't know what it should behave. 
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        var windowEdge = ResizeDirection switch
        {
            ResizeDirection.Top => WindowEdge.North,
            ResizeDirection.TopRight => WindowEdge.NorthEast,
            ResizeDirection.Right => WindowEdge.East,
            ResizeDirection.BottomRight => WindowEdge.SouthEast,
            ResizeDirection.Bottom => WindowEdge.South,
            ResizeDirection.BottomLeft => WindowEdge.SouthWest,
            ResizeDirection.Left => WindowEdge.West,
            ResizeDirection.TopLeft => WindowEdge.NorthWest,
            _ => throw new ArgumentOutOfRangeException()
        };
        _window.BeginResizeDrag(windowEdge, e);
    }
}
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace OvoUi.Controls;

/// <summary>
/// Applies an elastic stretch while its child is dragged and springs back when released.
/// </summary>
public class ElasticDecorator : Decorator
{
    private const double DragThreshold = 3;

    private readonly ElasticTransformController _controller;
    private bool _isPointerDown;
    private bool _isDragging;
    private Point _pointerDownPosition;

    static ElasticDecorator()
    {
        AffectsRender<ElasticDecorator>(ChildProperty);
    }

    public ElasticDecorator()
    {
        _controller = new ElasticTransformController(this);
        RenderTransformOrigin = RelativePoint.Center;

        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed)
            return;

        _isPointerDown = true;
        _isDragging = false;
        _pointerDownPosition = point.Position;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isPointerDown)
            return;

        var dragDelta = e.GetPosition(this) - _pointerDownPosition;
        if (!_isDragging &&
            (Math.Abs(dragDelta.X) > DragThreshold || Math.Abs(dragDelta.Y) > DragThreshold))
        {
            _isDragging = true;
            _controller.StopResetAnimation();
            e.Pointer.Capture(this);
        }

        if (!_isDragging)
            return;

        _controller.DragDelta = dragDelta;
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isPointerDown || e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        if (_isDragging)
            e.Handled = true;

        e.Pointer.Capture(null);
        ResetInteraction();
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        ResetInteraction();
        base.OnPointerCaptureLost(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _controller.StopResetAnimation(resetTransform: true);
        _isPointerDown = false;
        _isDragging = false;
        base.OnDetachedFromVisualTree(e);
    }

    private void ResetInteraction()
    {
        if (!_isPointerDown)
            return;

        _isPointerDown = false;
        _isDragging = false;
        _controller.Reset();
    }
}
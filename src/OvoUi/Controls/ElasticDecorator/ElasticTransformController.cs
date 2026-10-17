using System.Diagnostics;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace OvoUi.Controls;

internal sealed class ElasticTransformController
{
    private static readonly TimeSpan ResetDuration = TimeSpan.FromSeconds(0.382);

    private readonly Control _target;
    private readonly ElasticStretchCalculator _stretchCalculator = new();
    private readonly ScaleTransform _scaleTransform = new(1, 1);
    private readonly TranslateTransform _translateTransform = new();
    private readonly DispatcherTimer _resetTimer;
    private readonly Stopwatch _resetStopwatch = new();

    private Vector _dragDelta;
    private double _startScaleX;
    private double _startScaleY;
    private double _startTranslateX;
    private double _startTranslateY;

    public ElasticTransformController(Control target)
    {
        _target = target;
        _target.SizeChanged += (_, e) => _stretchCalculator.OriginalSize = e.NewSize;
        Transform = new TransformGroup
        {
            Children = new Transforms { _scaleTransform, _translateTransform }
        };
        _resetTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, OnResetTimerTick);
    }

    public Transform Transform { get; }

    public Vector DragDelta
    {
        get => _dragDelta;
        set
        {
            if (!ReferenceEquals(_target.RenderTransform, Transform))
                _target.RenderTransform = Transform;

            _dragDelta = value;
            _stretchCalculator.DragDelta = value;
            _stretchCalculator.Calculate();

            if (_target.Bounds.Width > 0 && _target.Bounds.Height > 0)
            {
                _scaleTransform.ScaleX = 1 + _stretchCalculator.StretchX / _target.Bounds.Width;
                _scaleTransform.ScaleY = 1 + _stretchCalculator.StretchY / _target.Bounds.Height;
            }

            _translateTransform.X = Math.Sign(value.X) * _stretchCalculator.OffsetX;
            _translateTransform.Y = Math.Sign(value.Y) * _stretchCalculator.OffsetY;
        }
    }

    public void Reset()
    {
        if (DragDelta == default)
            return;

        _dragDelta = default;
        StopResetAnimation();
        _startScaleX = _scaleTransform.ScaleX;
        _startScaleY = _scaleTransform.ScaleY;
        _startTranslateX = _translateTransform.X;
        _startTranslateY = _translateTransform.Y;
        _resetStopwatch.Restart();
        _resetTimer.Start();
    }

    public void StopResetAnimation(bool resetTransform = false)
    {
        _resetTimer.Stop();
        _resetStopwatch.Reset();

        if (!resetTransform)
            return;

        _dragDelta = default;
        SetTransform(1, 1, 0, 0);
        _target.RenderTransform = null;
    }

    private void OnResetTimerTick(object? sender, EventArgs e)
    {
        var progress = Math.Min(1, _resetStopwatch.Elapsed.TotalMilliseconds / ResetDuration.TotalMilliseconds);
        var easedProgress = new BackEaseOut().Ease(progress);

        SetTransform(
            Lerp(_startScaleX, 1, easedProgress),
            Lerp(_startScaleY, 1, easedProgress),
            Lerp(_startTranslateX, 0, easedProgress),
            Lerp(_startTranslateY, 0, easedProgress));

        if (progress >= 1)
        {
            StopResetAnimation();
            _target.RenderTransform = null;
        }
    }

    private void SetTransform(double scaleX, double scaleY, double translateX, double translateY)
    {
        _scaleTransform.ScaleX = scaleX;
        _scaleTransform.ScaleY = scaleY;
        _translateTransform.X = translateX;
        _translateTransform.Y = translateY;
    }

    private static double Lerp(double start, double end, double progress) => start + (end - start) * progress;
}
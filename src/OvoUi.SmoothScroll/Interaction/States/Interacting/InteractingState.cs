using System;
using Avalonia;
using Avalonia.Input;

namespace OvoUi.SmoothScroll.Interaction;

internal sealed class InteractingState : InteractionTrackerState
{
    private const double ReferenceRange = 2000;
    private const double Tension = 0.5;

    internal override string Name => "InteractingState";

    private double _previousScale;
    private Point _previousOrigin;
    private Vector3D _position;

    public InteractingState(ServerInteractionTracker interactionTracker) : base(interactionTracker)
    {
        _previousScale = interactionTracker.Scale;
        _position = GetOriginalPoint(interactionTracker.Position, _interactionTracker.MinPosition,
            _interactionTracker.MaxPosition);
        EnterState();
    }

    protected override void EnterState()
    {
        _interactionTracker.NotifyInteractingStateEntered(requestId: 0, isFromBinding: false);
    }

    internal override void BeginUserManipulation(Point position, IPointer pointer)
    {
        // This probably shouldn't happen.
        // We ignore.
        //if (this.Log().IsEnabled(LogLevel.Error))
        //{
        //    this.Log().Error("Unexpected StartUserManipulation while in interacting state");
        //}
    }

    internal override void CompleteUserManipulation()
    {
        _interactionTracker.ChangeState(new ActiveInputInertiaState(_interactionTracker, default, requestId: 0));
    }

    internal override void AddScaleVelocity(Point origin, double scaleDelta)
    {
        if (scaleDelta <= 0 || double.IsNaN(scaleDelta) || double.IsInfinity(scaleDelta))
        {
            return;
        }

        var currentPosition = ApplyOriginTranslation(origin, _position, out var positionChanged);

        var targetScale = _previousScale * scaleDelta;
        var clampedScale = Math.Clamp(targetScale, _interactionTracker.MinScale, _interactionTracker.MaxScale);
        var scaleChanged = Math.Abs(clampedScale - _previousScale) > double.Epsilon;

        _position = currentPosition;

        if (positionChanged)
        {
            UpdateTrackerPosition(_position);
        }

        if (scaleChanged)
        {
            ApplyScale(origin, clampedScale, currentPosition);
        }
        else if (!positionChanged)
        {
            UpdateTrackerPosition(_position);
        }

        _previousOrigin = origin;
    }

    internal override void ApplyManipulationDelta(Vector translationDelta)
    {
        _position += new Vector3D((float)translationDelta.X, (float)translationDelta.Y, 0);
        UpdateTrackerPosition(_position);
    }

    private Vector3D ApplyOriginTranslation(Point origin, Vector3D position, out bool positionChanged)
    {
        positionChanged = false;

        if (_previousOrigin == default)
        {
            return position;
        }

        var originDelta = origin - _previousOrigin;
        if (originDelta == default)
        {
            return position;
        }

        positionChanged = true;

        return new Vector3D(
            position.X - (float)originDelta.X,
            position.Y - (float)originDelta.Y,
            position.Z);
    }

    private void ApplyScale(Point origin, double scale, Vector3D position)
    {
        _position = ScalePosition(position, origin, scale / _previousScale);
        _interactionTracker.SetScale(scale, new Vector3D(origin.X, origin.Y, 0), 0);
        _previousScale = scale;
    }

    private static Vector3D ScalePosition(Vector3D position, Point origin, double scaleRatio)
    {
        var deltaX = (origin.X + position.X) * (1 - scaleRatio);
        var deltaY = (origin.Y + position.Y) * (1 - scaleRatio);

        return new Vector3D(
            position.X - deltaX,
            position.Y - deltaY,
            position.Z);
    }

    private void SyncPositionFromTracker()
    {
        _position = GetOriginalPoint(_interactionTracker.Position, _interactionTracker.MinPosition,
            _interactionTracker.MaxPosition);
    }

    private void UpdateTrackerPosition(Vector3D position)
    {
        var modifiedPosition =
            GetElasticPoint(position, _interactionTracker.MinPosition, _interactionTracker.MaxPosition);
        _interactionTracker.SetPosition(modifiedPosition, requestId: 0);
    }

    internal override void StartInertia(Vector linearVelocity)
    {
        _interactionTracker.ChangeState(new ActiveInputInertiaState(
            _interactionTracker,
            new Vector3D((float)linearVelocity.X, (float)linearVelocity.Y, 0),
            requestId: 0));
    }

    internal override void ApplyWheelDelta(Vector delta)
    {
    }

    internal override void TryUpdatePositionWithAdditionalVelocity(Vector3D velocityInPixelsPerSecond, int requestId)
    {
        _interactionTracker.NotifyRequestIgnored(requestId);
    }

    internal override void TryUpdatePosition(Vector3D value, InteractionTrackerClampingOption option, int requestId)
    {
        _interactionTracker.NotifyRequestIgnored(requestId);
    }

    internal override void ReceiveBoundsUpdate()
    {
        SyncPositionFromTracker();
    }

    public static Vector3D GetElasticPoint(Vector3D current, Vector3D min, Vector3D max, double tension = Tension)
    {
        return new Vector3D(
            GetElasticCoordinate(current.X, min.X, max.X, tension),
            GetElasticCoordinate(current.Y, min.Y, max.Y, tension),
            GetElasticCoordinate(current.Z, min.Z, max.Z, tension));
    }

    private static double GetElasticCoordinate(double current, double min, double max, double tension)
    {
        (min, max) = GetOrderedBounds(min, max);

        if (double.IsNaN(current))
        {
            return min;
        }

        if (current < min)
        {
            return min - CalculateOffset(min - current, tension);
        }

        if (current > max)
        {
            return max + CalculateOffset(current - max, tension);
        }

        return current;
    }

    public static Vector3D GetOriginalPoint(Vector3D elasticPoint, Vector3D min, Vector3D max, double tension = Tension)
    {
        return new Vector3D(
            GetOriginalCoordinate(elasticPoint.X, min.X, max.X, tension),
            GetOriginalCoordinate(elasticPoint.Y, min.Y, max.Y, tension),
            GetOriginalCoordinate(elasticPoint.Z, min.Z, max.Z, tension));
    }

    private static double GetOriginalCoordinate(double elasticPoint, double min, double max, double tension)
    {
        (min, max) = GetOrderedBounds(min, max);

        if (elasticPoint < min)
        {
            var offset = CalculateInverseOffset(min - elasticPoint, tension);
            return SubtractWithSaturation(min, offset);
        }

        if (elasticPoint > max)
        {
            var offset = CalculateInverseOffset(elasticPoint - max, tension);
            return AddWithSaturation(max, offset);
        }

        return double.IsNaN(elasticPoint) ? min : elasticPoint;
    }

    private static (double Min, double Max) GetOrderedBounds(double min, double max)
    {
        var minIsNaN = double.IsNaN(min);
        var maxIsNaN = double.IsNaN(max);

        if (minIsNaN && maxIsNaN)
        {
            return (0, 0);
        }

        if (minIsNaN)
        {
            min = max;
        }

        if (maxIsNaN)
        {
            max = min;
        }

        return min <= max ? (min, max) : (max, min);
    }

    private static double CalculateOffset(double distance, double tension)
    {
        if (distance <= 0 || tension <= 0 || double.IsNaN(distance) || double.IsNaN(tension))
        {
            return 0;
        }

        if (double.IsPositiveInfinity(distance))
        {
            return ReferenceRange * tension;
        }

        return (distance / (distance + ReferenceRange)) * ReferenceRange * tension;
    }

    private static double CalculateInverseOffset(double resultOffset, double tension)
    {
        if (resultOffset <= 0 || tension <= 0 || double.IsNaN(resultOffset) || double.IsNaN(tension))
        {
            return 0;
        }

        double limit = ReferenceRange * tension;

        if (limit <= 0 || double.IsNaN(limit) || double.IsPositiveInfinity(resultOffset) || resultOffset >= limit)
        {
            return double.MaxValue;
        }

        var denominator = limit / resultOffset - 1.0;
        if (denominator <= 0 || double.IsNaN(denominator))
        {
            return double.MaxValue;
        }

        var offset = ReferenceRange / denominator;
        return double.IsNaN(offset) || double.IsInfinity(offset) ? double.MaxValue : offset;
    }

    private static double AddWithSaturation(double value, double offset)
    {
        var result = value + offset;
        return double.IsNaN(result) || double.IsPositiveInfinity(result) ? double.MaxValue : result;
    }

    private static double SubtractWithSaturation(double value, double offset)
    {
        var result = value - offset;
        return double.IsNaN(result) || double.IsNegativeInfinity(result) ? double.MinValue : result;
    }
}
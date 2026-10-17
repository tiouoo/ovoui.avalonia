using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OvoUi.SmoothScroll.Interaction;

namespace OvoUi.Demo.Pages;

public partial class InteractionTrackerPage : UserControl, IInteractionTrackerOwner
{
    private InteractionTracker? _tracker;
    private InputElementInteractionSource? _source;
    private bool _needsInitialPosition;

    public InteractionTrackerPage()
    {
        InitializeComponent();
        DataContext = new InteractionTrackerDemoModel();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Dispatcher.UIThread.Post(() =>
        {
            if (this.IsAttachedToVisualTree())
                Surface_OnLoaded(null, new RoutedEventArgs());
        }, DispatcherPriority.Loaded);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Surface_OnUnloaded(null, new RoutedEventArgs());
        base.OnDetachedFromVisualTree(e);
    }

    private void Surface_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (_tracker is not null || ElementComposition.GetElementVisual(InputSurface) is not { } surface)
            return;
        _tracker = surface.Compositor.CreateInteractionTracker(this);
        _needsInitialPosition = true;
        _source = new InputElementInteractionSource(InputSurface, _tracker)
            { IsMousePanningEnabled = true, IsMousePanningInertiaEnabled = true };
        UpdateBounds();
        UpdateInertia();
        var primary = surface.Compositor.CreateExpressionAnimation("tracker.Position");
        primary.SetReferenceParameter("tracker", _tracker);
        ElementComposition.GetElementVisual(PrimaryTile)!.StartAnimation("Translation", primary);
        var follower = surface.Compositor.CreateExpressionAnimation("tracker.Position * 0.5");
        follower.SetReferenceParameter("tracker", _tracker);
        ElementComposition.GetElementVisual(FollowerTile)!.StartAnimation("Translation", follower);
        var reversed =
            surface.Compositor.CreateExpressionAnimation(
                "Vector3(tracker.Position.Y, tracker.Position.X, tracker.Position.Z)");
        reversed.SetReferenceParameter("tracker", _tracker);
        ElementComposition.GetElementVisual(ReversedTile)!.StartAnimation("Translation", reversed);
        // The initial loaded event can precede the final layout of the showcase.
        var tracker = _tracker;
        Dispatcher.UIThread.Post(() =>
        {
            if (_tracker != tracker || !InputSurface.IsAttachedToVisualTree()) return;
            UpdateBounds();
        }, DispatcherPriority.Background);
    }

    private void Surface_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        ElementComposition.GetElementVisual(PrimaryTile)?.StopAnimation("Translation");
        ElementComposition.GetElementVisual(FollowerTile)?.StopAnimation("Translation");
        ElementComposition.GetElementVisual(ReversedTile)?.StopAnimation("Translation");
        _source?.Dispose();
        _tracker?.Dispose();
        _source = null;
        _tracker = null;
    }

    private void Surface_OnSizeChanged(object? sender, SizeChangedEventArgs e) => UpdateBounds();

    private void UpdateBounds()
    {
        if (_tracker is null || InputSurface.Bounds.Width <= 0 || InputSurface.Bounds.Height <= 0)
            return;
        // Keep the position and its swapped-axis projection inside the same canvas.
        var side = Math.Max(0, Math.Min(InputSurface.Bounds.Width - 110, InputSurface.Bounds.Height - 80));
        _tracker.MaxPosition = new Vector3D(side, side, 0);
        if (_needsInitialPosition && _tracker.MaxPosition.X > 0 && _tracker.MaxPosition.Y > 0)
        {
            _needsInitialPosition = false;
            _tracker.TryUpdatePosition(Vector3D.Min(new Vector3D(120, 80, 0), _tracker.MaxPosition));
        }
    }

    private void Inertia_OnChanged(object? sender, RoutedEventArgs e) => UpdateInertia();

    private void UpdateInertia()
    {
        if (_source is null) return;
        var mode = InertiaToggle.IsChecked == true
            ? InteractionSourceMode.EnabledWithInertia
            : InteractionSourceMode.EnabledWithoutInertia;
        _source.PositionXSourceMode = mode;
        _source.PositionYSourceMode = mode;
    }

    private void Move_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_tracker is null) return;
        switch ((sender as Button)?.Tag as string)
        {
            case "by": _tracker.TryUpdatePositionBy(new Vector3D(80, 40, 0)); break;
            case "animate":
                var animation = _tracker.Compositor.CreateVector3DKeyFrameAnimation();
                animation.Duration = TimeSpan.FromMilliseconds(700);
                animation.InsertKeyFrame(1, _tracker.MaxPosition, new CubicEaseOut());
                _tracker.TryUpdatePositionWithAnimation(animation);
                break;
            default: _tracker.TryUpdatePosition(default); break;
        }
    }

    public void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args) =>
        TrackerStatus.Text =
            $"Position: {args.Position.X:F0}, {args.Position.Y:F0}   Scale: {args.Scale:F2}   Request: {args.RequestId}";

    public void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args)
    {
    }

    public void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args)
    {
    }

    public void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args)
    {
    }

    public void CustomAnimationStateEntered(InteractionTracker sender,
        InteractionTrackerCustomAnimationStateEnteredArgs args)
    {
    }

    public void RequestIgnored(InteractionTracker sender, InteractionTrackerRequestIgnoredArgs args) =>
        TrackerStatus.Text = "The request was ignored while a gesture was active.";
}

public class InteractionTrackerDemoModel
{
    public string SourceCode => """
                                using OvoUi.SmoothScroll.Interaction;
                                // Create on Loaded, after composition visuals exist.
                                var visual = ElementComposition.GetElementVisual(InputSurface)!;
                                var tracker = visual.Compositor.CreateInteractionTracker(owner: null);
                                tracker.MinPosition = default;
                                var side = Math.Max(0, Math.Min(InputSurface.Bounds.Width - 110, InputSurface.Bounds.Height - 80));
                                tracker.MaxPosition = new Vector3D(side, side, 0);
                                var source = new InputElementInteractionSource(InputSurface, tracker)
                                {
                                    IsMousePanningEnabled = true,
                                    IsMousePanningInertiaEnabled = true,
                                    PositionXSourceMode = InteractionSourceMode.EnabledWithInertia,
                                    PositionYSourceMode = InteractionSourceMode.EnabledWithInertia
                                };
                                var expression = visual.Compositor.CreateExpressionAnimation("tracker.Position");
                                expression.SetReferenceParameter("tracker", tracker);
                                ElementComposition.GetElementVisual(PrimaryTile)!.StartAnimation("Translation", expression);
                                var follower = visual.Compositor.CreateExpressionAnimation("tracker.Position * 0.5");
                                follower.SetReferenceParameter("tracker", tracker);
                                ElementComposition.GetElementVisual(FollowerTile)!.StartAnimation("Translation", follower);
                                var reversed = visual.Compositor.CreateExpressionAnimation("Vector3(tracker.Position.Y, tracker.Position.X, tracker.Position.Z)");
                                reversed.SetReferenceParameter("tracker", tracker);
                                ElementComposition.GetElementVisual(ReversedTile)!.StartAnimation("Translation", reversed);
                                tracker.TryUpdatePosition(new Vector3D(120, 80, 0));
                                tracker.TryUpdatePositionBy(new Vector3D(80, 40, 0));
                                var animation = visual.Compositor.CreateVector3DKeyFrameAnimation();
                                animation.Duration = TimeSpan.FromMilliseconds(700);
                                animation.InsertKeyFrame(1, tracker.MaxPosition, new CubicEaseOut());
                                tracker.TryUpdatePositionWithAnimation(animation);
                                // For the checkbox, set both source modes to EnabledWithoutInertia.
                                // Update MaxPosition on SizeChanged. On Unloaded:
                                ElementComposition.GetElementVisual(PrimaryTile)?.StopAnimation("Translation");
                                ElementComposition.GetElementVisual(FollowerTile)?.StopAnimation("Translation");
                                ElementComposition.GetElementVisual(ReversedTile)?.StopAnimation("Translation");
                                source.Dispose();
                                tracker.Dispose();
                                // Pass an IInteractionTrackerOwner to receive values and state callbacks.
                                """;
}
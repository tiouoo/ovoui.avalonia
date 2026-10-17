using Avalonia;
using Avalonia.Utilities;

namespace OvoUi.SmoothScroll.Interaction;

internal sealed class PointerWheelInertiaState : InertiaState
{
    public PointerWheelInertiaState(
        ServerInteractionTracker interactionTracker,
        Vector3D translationVelocities,
        int requestId) : base(interactionTracker, requestId)
    {
        Handler = new PointerWheelInertiaHandler(
            interactionTracker.Compositor,
            interactionTracker,
            translationVelocities);
        EnterState();
    }
}
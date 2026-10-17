using Avalonia;
using Avalonia.Utilities;

namespace OvoUi.SmoothScroll.Interaction;

internal sealed class ActiveInputInertiaState : InertiaState
{
    public ActiveInputInertiaState(
        ServerInteractionTracker interactionTracker,
        Vector3D translationVelocities,
        int requestId) : base(interactionTracker, requestId)
    {
        Handler = new ActiveInputInertiaHandler(
            interactionTracker.Compositor,
            interactionTracker,
            translationVelocities,
            RequestId);

        EnterState();
    }
}

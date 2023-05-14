using Lotus.Roles.Interactions.Interfaces;

namespace Lotus.Roles.Interactions;

public class DelayedInteraction : DirectInteraction, IDelayedInteraction
{
    private readonly float delay;

    public DelayedInteraction(Intent intent, float delay, CustomRole? customRole = null) : base(intent, customRole)
    {
        this.delay = delay;
    }

    public float Delay() => delay;
}
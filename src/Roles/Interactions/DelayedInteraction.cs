using Lotus.Roles.Interactions.Interfaces;

namespace Lotus.Roles.Interactions;

public class DelayedInteraction : LotusInteraction, IDelayedInteraction
{
    private readonly float delay;

    public DelayedInteraction(Intent intent, float delay, CustomRole customRole) : base(intent, customRole)
    {
        this.delay = delay;
    }

    public float Delay() => delay;

    public override Interaction Modify(Intent intent) => new DelayedInteraction(intent, delay, Emitter());
}
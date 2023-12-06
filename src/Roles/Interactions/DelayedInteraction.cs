extern alias JBAnnotations;
using JBAnnotations::JetBrains.Annotations;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles2;

namespace Lotus.Roles.Interactions;

[UsedImplicitly]
public class DelayedInteraction : LotusInteraction, IDelayedInteraction
{
    private readonly float delay;

    public DelayedInteraction(Intent intent, float delay, UnifiedRoleDefinition roleDefinition) : base(intent, roleDefinition)
    {
        this.delay = delay;
    }

    public float Delay() => delay;

    public override Interaction Modify(Intent intent) => new DelayedInteraction(intent, delay, Emitter());
}
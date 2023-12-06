using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles2;

namespace Lotus.Roles.Interactions;

public class RangedInteraction : LotusInteraction, IRangedInteraction
{
    private readonly float distance;

    public RangedInteraction(Intent intent, float distance, UnifiedRoleDefinition roleDefinition) : base(intent, roleDefinition)
    {
        this.distance = distance;
    }

    public override Interaction Modify(Intent intent) => new RangedInteraction(intent, distance, Emitter());

    public float Distance() => distance;
}
using Lotus.Roles.Interactions.Interfaces;

namespace Lotus.Roles.Interactions;

public class RangedInteraction : LotusInteraction, IRangedInteraction
{
    private readonly float distance;

    public RangedInteraction(Intent intent, float distance, CustomRole customRole) : base(intent, customRole)
    {
        this.distance = distance;
    }

    public override Interaction Modify(Intent intent) => new RangedInteraction(intent, distance, Emitter());

    public float Distance() => distance;
}
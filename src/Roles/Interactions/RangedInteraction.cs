using Lotus.Roles.Interactions.Interfaces;

namespace Lotus.Roles.Interactions;

public class RangedInteraction : DirectInteraction, IRangedInteraction
{
    private readonly float distance;

    public RangedInteraction(Intent intent, float distance, CustomRole customRole) : base(intent, customRole)
    {
        this.distance = distance;
    }

    public float Distance() => distance;
}
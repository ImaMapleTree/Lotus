using Lotus.Roles.Interactions.Interfaces;

namespace Lotus.Roles.Interactions;

public class UnblockedInteraction : LotusInteraction, IUnblockedInteraction
{
    public UnblockedInteraction(Intent intent, CustomRole customRole) : base(intent, customRole)
    {
        IsPromised = true;
    }

    public override Interaction Modify(Intent intent) => new UnblockedInteraction(intent, Emitter());
}
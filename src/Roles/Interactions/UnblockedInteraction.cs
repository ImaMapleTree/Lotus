using Lotus.Roles.Interactions.Interfaces;

namespace Lotus.Roles.Interactions;

public class UnblockedInteraction : DirectInteraction, IUnblockedInteraction
{
    public UnblockedInteraction(Intent intent, CustomRole customRole) : base(intent, customRole)
    {
    }

    public override Interaction Modify(Intent intent) => new UnblockedInteraction(intent, Emitter());
}
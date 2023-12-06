using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles2;

namespace Lotus.Roles.Interactions;

public class UnblockedInteraction : LotusInteraction, IUnblockedInteraction
{
    public UnblockedInteraction(Intent intent, RoleDefinition roleDefinition) : this(intent, roleDefinition.Handle) {}

    public UnblockedInteraction(Intent intent, UnifiedRoleDefinition roleDefinition) : base(intent, roleDefinition)
    {
        IsPromised = true;
    }

    public override Interaction Modify(Intent intent) => new UnblockedInteraction(intent, Emitter());
}
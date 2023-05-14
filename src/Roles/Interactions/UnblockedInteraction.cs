using Lotus.Roles.Interactions.Interfaces;

namespace Lotus.Roles.Interactions;

public class UnblockedInteraction : DirectInteraction, IUnblockedInteraction
{
    public UnblockedInteraction(Intent intent, CustomRole? customRole = null) : base(intent, customRole)
    {
    }
}
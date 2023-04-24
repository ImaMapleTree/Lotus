using TOHTOR.Roles.Interactions.Interfaces;

namespace TOHTOR.Roles.Interactions;

public class IndirectInteraction : DirectInteraction, IIndirectInteraction
{
    public IndirectInteraction(Intent intent, CustomRole? customRole = null) : base(intent, customRole)
    {
    }
}
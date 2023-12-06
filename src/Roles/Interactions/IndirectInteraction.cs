extern alias JBAnnotations;
using JBAnnotations::JetBrains.Annotations;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles2;

namespace Lotus.Roles.Interactions;

[UsedImplicitly]
public class IndirectInteraction : LotusInteraction, IIndirectInteraction
{
    public IndirectInteraction(Intent intent, UnifiedRoleDefinition? roleDefinition = null) : base(intent, roleDefinition)
    {
    }

    public override Interaction Modify(Intent intent) => new IndirectInteraction(intent, Emitter());
}
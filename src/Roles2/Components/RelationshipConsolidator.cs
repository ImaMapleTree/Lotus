using System.Collections.Generic;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Roles2.Interfaces;

namespace Lotus.Roles2.Components;

public abstract class RelationshipConsolidator: IUnifiedDefinitionAware, IInstantiatedComponentAware<IRelationshipComponent>
{
    protected UnifiedRoleDefinition RoleDefinition { get; private set; } = null!;
    protected List<IRelationshipComponent> RelationshipComponents = new();

    public abstract Relation Relationship(UnifiedRoleDefinition roleDefinition);

    public abstract Relation Relationship(IFaction faction);

    public virtual void ReceiveTargetInstantiatedComponents(List<IRelationshipComponent> components)
    {
        RelationshipComponents = components;
        components.Reverse();
    }

    public virtual IRoleComponent Instantiate(SetupHelper setupHelper, PlayerControl player) => setupHelper.Clone(this);

    public void SetUnifiedDefinition(UnifiedRoleDefinition unifiedRoleDefinition) => RoleDefinition = unifiedRoleDefinition;
}
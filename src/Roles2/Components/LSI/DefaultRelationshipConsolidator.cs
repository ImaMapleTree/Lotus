using Lotus.Factions;
using Lotus.Factions.Interfaces;

namespace Lotus.Roles2.Components.LSI;

public class DefaultRelationshipConsolidator: RelationshipConsolidator
{
    public override Relation Relationship(UnifiedRoleDefinition roleDefinition)
    {
        foreach (IRelationshipComponent relationshipComponent in RelationshipComponents)
            if (relationshipComponent.Relationship(roleDefinition, out Relation relation)) return relation;

        return Relation.None;
    }

    public override Relation Relationship(IFaction faction)
    {
        foreach (IRelationshipComponent relationshipComponent in RelationshipComponents)
            if (relationshipComponent.Relationship(faction, out Relation relation)) return relation;

        return Relation.None;
    }
}
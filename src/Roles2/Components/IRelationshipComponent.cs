using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Roles2.Interfaces;

namespace Lotus.Roles2.Components;

public interface IRelationshipComponent: IRoleComponent
{
    /// <summary>
    /// Gets the <see cref="Relation"/> between the passed in role, and definition owning this component.
    /// Importantly, this function returns a <code>bool</code> that represents if the relationship was actually checked or not.
    /// This is used by a <see cref="RelationshipConsolidator"/> to determine if it should continue trying to check relationships.
    /// </summary>
    /// <param name="otherDefinition">the definition to check the relationship against</param>
    /// <param name="relation">the relation between the owning definition, and the provided <code>otherDefinition</code></param>
    /// <returns>true if the relationship was checked, and "final", false if the relationship is irrelevant</returns>
    public bool Relationship(UnifiedRoleDefinition otherDefinition, out Relation relation)
    {
        relation = Relationship(otherDefinition);
        return true;
    }

    /// <summary>
    /// Gets the <see cref="Relation"/> between th passed in faction, and definition owning this component.
    /// Importantly, this function returns a <code>bool</code> that represents if the relationship was actually checked or not.
    /// This is used by a <see cref="RelationshipConsolidator"/> to determine if it should continue trying to check relationships.
    /// </summary>
    /// <param name="otherFaction">the faction to check the relationship against</param>
    /// <param name="relation">the relation between the owning definition, and the provided <code>otherFaction</code></param>
    /// <returns>true if the relationship was checked, and "final", false if the relationship is irrelevant</returns>
    public bool Relationship(IFaction otherFaction, out Relation relation)
    {
        relation = Relationship(otherFaction);
        return true;
    }

    public Relation Relationship(UnifiedRoleDefinition otherDefinition) => Relationship(otherDefinition.Faction);

    public Relation Relationship(IFaction faction);
}
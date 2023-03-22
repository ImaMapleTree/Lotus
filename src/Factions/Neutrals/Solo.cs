using TOHTOR.Factions.Interfaces;

namespace TOHTOR.Factions.Neutrals;

public class Solo : Faction<Solo>
{
    private string factionName;

    public Solo(string? factionName = null)
    {
        this.factionName = factionName ?? "Solo";
    }

    public override string Name() => this.factionName;

    public override Relation Relationship(Solo sameFaction) => Relation.None;

    public override bool AlliesSeeRole() => false;

    public override Relation RelationshipOther(IFaction other) => Relation.None;
}
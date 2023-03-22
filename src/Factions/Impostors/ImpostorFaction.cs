using TOHTOR.Factions.Crew;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Factions.Neutrals;
using TOHTOR.Factions.Undead;

namespace TOHTOR.Factions.Impostors;

public class ImpostorFaction : Faction<ImpostorFaction>
{
    public override Relation Relationship(ImpostorFaction sameFaction) => Relation.FullAllies;

    public override bool AlliesSeeRole() => true;

    public override Relation RelationshipOther(IFaction other)
    {
        return other switch
        {
            TheUndead => Relation.None,
            Crewmates => Relation.None,
            Solo => Relation.None,
            _ => other.Relationship(this)
        };
    }
}
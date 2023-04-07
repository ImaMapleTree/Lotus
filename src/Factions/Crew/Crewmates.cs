using TOHTOR.Factions.Impostors;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Factions.Neutrals;
using TOHTOR.Factions.Undead;
using UnityEngine;

namespace TOHTOR.Factions.Crew;

public class Crewmates : Faction<Crewmates>
{
    public override Relation Relationship(Crewmates sameFaction) => Relation.FullAllies;

    public override bool AlliesSeeRole() => false;

    public override Color FactionColor() => new(0.71f, 0.94f, 1f);

    public override Relation RelationshipOther(IFaction other)
    {
        return other switch
        {
            TheUndead => Relation.None,
            ImpostorFaction => Relation.None,
            Solo => Relation.None,
            _ => other.Relationship(this)
        };
    }
}
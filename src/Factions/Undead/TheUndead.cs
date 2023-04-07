using TOHTOR.Factions.Crew;
using TOHTOR.Factions.Impostors;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Factions.Neutrals;
using UnityEngine;

namespace TOHTOR.Factions.Undead;

public abstract partial class TheUndead : Faction<TheUndead>
{
    public override string Name() => "The Undead";

    public override Relation Relationship(TheUndead sameFaction) => Relation.FullAllies;

    public override Relation RelationshipOther(IFaction other)
    {
        return other switch
        {
            ImpostorFaction => Relation.None,
            Crewmates => Relation.None,
            Solo => Relation.None,
            _ => other.Relationship(this)
        };
    }

    public override Color FactionColor() => new(0.59f, 0.76f, 0.36f);
}
using Lotus.Factions.Crew;
using Lotus.Factions.Impostors;
using Lotus.Factions.Interfaces;
using Lotus.Factions.Neutrals;
using UnityEngine;

namespace Lotus.Factions.Undead;

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
            Neutral => Relation.None,
            _ => other.Relationship(this)
        };
    }

    public override Color FactionColor() => new(0.59f, 0.76f, 0.36f);
}
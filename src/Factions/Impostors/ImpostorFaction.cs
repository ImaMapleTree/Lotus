using Lotus.Factions.Crew;
using Lotus.Factions.Interfaces;
using Lotus.Factions.Neutrals;
using Lotus.Factions.Undead;
using UnityEngine;

namespace Lotus.Factions.Impostors;

public class ImpostorFaction : Faction<ImpostorFaction>
{
    public override Relation Relationship(ImpostorFaction sameFaction) => Relation.FullAllies;

    public override bool CanSeeRole(PlayerControl player) => true;

    public override Color FactionColor() => Color.red;

    public override string Name() => "Impostors";

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
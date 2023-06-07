using Lotus.Factions.Impostors;
using Lotus.Factions.Interfaces;
using Lotus.Factions.Neutrals;
using Lotus.Factions.Undead;
using UnityEngine;

namespace Lotus.Factions.Crew;

public class Crewmates : Faction<Crewmates>
{
    public override Relation Relationship(Crewmates sameFaction) => Relation.FullAllies;

    public override bool CanSeeRole(PlayerControl player) => false;

    public override Color FactionColor() => ModConstants.Palette.CrewmateColor;

    public override Relation RelationshipOther(IFaction other)
    {
        return other switch
        {
            TheUndead => Relation.None,
            ImpostorFaction => Relation.None,
            Neutral => Relation.None,
            _ => other.Relationship(this)
        };
    }
}
using Lotus.Factions;
using Lotus.Roles.Subroles;
using UnityEngine;

namespace Lotus.Roles.RoleGroups.Crew;


public class Vigilante: Guesser
{
    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .Faction(FactionInstances.Crewmates)
            .RoleColor(new Color(0.89f, 0.88f, 0.52f));
}
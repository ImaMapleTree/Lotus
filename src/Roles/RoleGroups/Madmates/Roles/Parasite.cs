using TOHTOR.Factions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;

namespace TOHTOR.Roles.RoleGroups.Madmates.Roles;

public class Parasite : Shapeshifter
{
    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        canSabotage = true;
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .SpecialType(SpecialType.Madmate)
            .RoleColor(new Color(0.73f, 0.18f, 0.02f))
            .Faction(FactionInstances.Madmates);
}
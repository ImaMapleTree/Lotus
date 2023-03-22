using AmongUs.GameOptions;
using TOHTOR.Extensions;
using TOHTOR.Options;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;

namespace TOHTOR.Roles.Subroles;

public class Diseased: Subrole
{
    [RoleAction(RoleActionType.MyDeath)]
    private void DiseasedDies(PlayerControl killer)
    {
        CustomRole role = killer.GetCustomRole();
        float killCooldown = role is RoleGroups.Vanilla.Impostor imp ? imp.KillCooldown : DesyncOptions.OriginalHostOptions.GetFloat(FloatOptionNames.KillCooldown);
        role.AddOverride(new GameOptionOverride(Override.KillCooldown, killCooldown * 2));
    }

    public override string? Identifier() => "★";

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.42f, 0.4f, 0.16f));

}
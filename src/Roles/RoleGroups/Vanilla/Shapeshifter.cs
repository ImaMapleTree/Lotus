using AmongUs.GameOptions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using UnityEngine;

namespace Lotus.Roles.RoleGroups.Vanilla;

public class Shapeshifter : Impostor
{
    protected float? ShapeshiftCooldown = null;
    protected float? ShapeshiftDuration = null;

    [RoleAction(RoleActionType.Attack, Subclassing = false)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(RoleTypes.Shapeshifter)
            .RoleColor(Color.red)
            .CanVent(true)
            .OptionOverride(Override.ShapeshiftCooldown, ShapeshiftCooldown)
            .OptionOverride(Override.ShapeshiftDuration, ShapeshiftDuration);
}
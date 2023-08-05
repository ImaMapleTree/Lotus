using AmongUs.GameOptions;
using Lotus.API;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.Builtins.Vanilla;

public class Shapeshifter : Impostor
{
    protected virtual float? ShapeshiftCooldown { get; set; }
    protected virtual float? ShapeshiftDuration { get; set; }

    [RoleAction(LotusActionType.Attack, Subclassing = false)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    protected GameOptionBuilder AddShapeshiftOptions(GameOptionBuilder builder)
    {
        return builder.SubOption(sub => sub.Name("Shapeshift Cooldown")
                .AddFloatRange(0, 120, 2.5f, 12, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => ShapeshiftCooldown = f)
                .Build())
            .SubOption(sub => sub.Name("Shapeshift Duration")
                .Value(1f)
                .AddFloatRange(2.5f, 120, 2.5f, 6, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => ShapeshiftDuration = f)
                .Build());
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(RoleTypes.Shapeshifter)
            .RoleColor(Color.red)
            .CanVent(true)
            .OptionOverride(Override.ShapeshiftCooldown, () => ShapeshiftCooldown ?? AUSettings.ShapeshifterCooldown())
            .OptionOverride(Override.ShapeshiftDuration, () => ShapeshiftDuration ?? AUSettings.ShapeshifterDuration());
}
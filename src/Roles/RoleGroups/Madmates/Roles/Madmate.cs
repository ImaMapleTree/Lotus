using AmongUs.GameOptions;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.Game;

namespace TOHTOR.Roles.RoleGroups.Madmates.Roles;

public class Madmate : Impostor
{
    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Can Sabotage")
                .BindBool(b => canSabotage = b)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(canSabotage ? RoleTypes.Impostor : RoleTypes.Engineer)
            .SpecialType(SpecialType.Madmate)
            .RoleColor(new Color(0.73f, 0.18f, 0.02f))
            .Faction(FactionInstances.Madmates);
}
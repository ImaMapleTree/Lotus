using AmongUs.GameOptions;
using Lotus.Factions;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Madmates.Roles;

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
            .RoleColor(ModConstants.Palette.MadmateColor)
            .Faction(FactionInstances.Madmates);
}
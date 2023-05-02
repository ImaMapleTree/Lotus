using AmongUs.GameOptions;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Options;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.Game;

namespace TOHTOR.Roles.RoleGroups.Madmates.Roles;

public abstract class MadCrewmate : Engineer
{
    private bool canVent;
    private bool impostorVision;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddTaskOverrideOptions(base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Has Impostor Vision")
                .AddOnOffValues()
                .BindBool(b => impostorVision = b)
                .Build()
            )
            .SubOption(sub => sub.Name("Can Vent")
                .AddOnOffValues()
                .BindBool(b => canVent = b)
                .SubOption(sub2 => sub2.Name("Vent Cooldown")
                    .BindFloat(f => this.VentCooldown = f)
                    .AddFloatRange(0, 60, 2.5f, 8, "s")
                    .Build())
                .SubOption(sub2 => sub2.Name("Vent Duration")
                    .BindFloat(f => this.VentDuration = f)
                    .Value(1f)
                    .AddFloatRange(2, 120, 2.5f, 4, "s")
                    .Build())
                .Build()));

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(canVent ? RoleTypes.Engineer : RoleTypes.Crewmate)
            .SpecialType(SpecialType.Madmate)
            .RoleColor(new Color(0.73f, 0.18f, 0.02f))
            .Faction(FactionInstances.Madmates)
            .OptionOverride(Override.CrewLightMod, () => AUSettings.CrewLightMod(), () => impostorVision);
}

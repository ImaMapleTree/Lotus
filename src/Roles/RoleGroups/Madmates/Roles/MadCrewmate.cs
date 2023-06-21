using AmongUs.GameOptions;
using Lotus.API;
using Lotus.Factions;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Madmates.Roles;

public abstract class MadCrewmate : Engineer
{
    private bool canVent;
    private bool impostorVision;

    public override bool TasksApplyToTotal() => false;

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
                    .AddFloatRange(0, 60, 2.5f, 8, GeneralOptionTranslations.SecondsSuffix)
                    .Build())
                .SubOption(sub2 => sub2.Name("Vent Duration")
                    .BindFloat(f => this.VentDuration = f)
                    .Value(1f)
                    .AddFloatRange(2, 120, 2.5f, 4, GeneralOptionTranslations.SecondsSuffix)
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

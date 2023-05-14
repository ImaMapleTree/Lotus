using Lotus.API;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class Jackal : NeutralKillingBase
{
    private bool canVent;
    private bool impostorVision;

    [RoleAction(RoleActionType.Attack)]
    public new bool TryKill(PlayerControl target) => base.TryKill(target);

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream))
            .SubOption(sub => sub
                .Name("Can Vent")
                .BindBool(v => canVent = v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .Name("Can Sabotage")
                .BindBool(v => canSabotage = v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .Name("Impostor Vision")
                .BindBool(v => impostorVision = v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0f, 0.71f, 0.92f))
            .CanVent(canVent)
            .OptionOverride(Override.ImpostorLightMod, () => AUSettings.CrewLightMod(), () => !impostorVision);
}
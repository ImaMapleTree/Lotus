using Lotus.API;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class Juggernaut : NeutralKillingBase
{
    private bool canVent;
    private bool impostorVision;
    private float decreaseBy;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (!base.TryKill(target)) return false;
        VentLogger.Trace($"Juggernaut Kill Cooldown {KillCooldown} => {KillCooldown - decreaseBy}");
        KillCooldown = Mathf.Clamp(KillCooldown - decreaseBy, 0f, int.MaxValue);
        SyncOptions();
        return true;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream))
            .SubOption(sub => sub
                .Name("Decrease Amount Each Kill")
                .BindFloat(v => decreaseBy = v)
                .AddFloatRange(0, 30, 0.5f, 5)
                .Build())
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
            .RoleColor(new Color(0.55f, 0f, 0.3f, 1f))
            .CanVent(canVent)
            .OptionOverride(Override.ImpostorLightMod, () => AUSettings.CrewLightMod(), () => !impostorVision);
}
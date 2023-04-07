using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.NeutralKilling;

[Localized("Roles.Werewolf")]
public class Werewolf: NeutralKillingBase
{
    private bool rampaging;

    [UIComponent(UI.Cooldown)]
    private Cooldown rampageDuration;

    [UIComponent(UI.Cooldown)]
    private Cooldown rampageCooldown;

    [Localized("Rampage")]
    private string rampagingString = "RAMPAGING";

    protected override void PostSetup()
    {
        base.PostSetup();
        MyPlayer.NameModel().GetComponentHolder<CooldownHolder>()[1].SetPrefix(RoleColor.Colorize(rampagingString + " "));
    }

    [RoleAction(RoleActionType.Attack)]
    public new bool TryKill(PlayerControl target) => rampaging && base.TryKill(target);

    [RoleAction(RoleActionType.OnPet)]
    private void EnterRampage()
    {
        if (rampageDuration.NotReady() || rampageCooldown.NotReady()) return;
        VentLogger.Trace($"{MyPlayer.GetNameWithRole()} Starting Rampage");
        rampaging = true;
        rampageDuration.Start();
        Async.Schedule(ExitRampage, rampageDuration.Duration);
    }

    [RoleAction(RoleActionType.RoundEnd)]
    private void ExitRampage()
    {
        VentLogger.Trace($"{MyPlayer.GetNameWithRole()} Ending Rampage");
        rampaging = false;
        rampageCooldown.Start();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Rampage Kill Cooldown")
                .AddFloatRange(1f, 60f, 2.5f, 2, "s")
                .BindFloat(f => KillCooldown = f)
                .Build())
            .SubOption(sub => sub.Name("Rampage Cooldown")
                .AddFloatRange(5f, 120f, 2.5f, 14, "s")
                .BindFloat(rampageCooldown.SetDuration)
                .Build())
            .SubOption(sub => sub.Name("Rampage Duration")
                .AddFloatRange(5f, 120f, 2.5f, 4, "s")
                .BindFloat(rampageDuration.SetDuration)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.66f, 0.4f, 0.16f));
}
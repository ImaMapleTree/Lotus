using AmongUs.GameOptions;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.Managers.History.Events;
using TOHTOR.Options;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Roles.RoleGroups.Impostors;

public class Janitor: Vanilla.Impostor
{
    private bool cleanOnKill;

    [UIComponent(UI.Cooldown)]
    private Cooldown cleanCooldown;

    [RoleAction(RoleActionType.Attack)]
    public new bool TryKill(PlayerControl target)
    {
        cleanCooldown.Start(OriginalOptions.KillCooldown());

        if (!cleanOnKill) return base.TryKill(target);

        if (MyPlayer.InteractWith(target, new SimpleInteraction(new FakeFatalIntent(), this)) is InteractionResult.Halt) return false;
        target.RpcExileV2();
        Game.GameHistory.AddEvent(new KillEvent(MyPlayer, target));
        Game.GameHistory.AddEvent(new GenericAbilityEvent(MyPlayer, $"{Color.red.Colorize(MyPlayer.UnalteredName())} cleaned {target.GetRoleColor().Colorize(target.UnalteredName())}."));
        return true;
    }

    [RoleAction(RoleActionType.SelfReportBody)]
    private void JanitorCleanBody(GameData.PlayerInfo target, ActionHandle handle)
    {
        if (cleanCooldown.NotReady()) return;
        handle.Cancel();
        cleanCooldown.Start();

        foreach (DeadBody deadBody in Object.FindObjectsOfType<DeadBody>())
            if (deadBody.ParentId == target.Object.PlayerId)
            {
                Game.GameStates.UnreportableBodies.Add(target.PlayerId);
                Object.Destroy(deadBody.gameObject);
            }

        MyPlayer.RpcGuardAndKill(MyPlayer);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Clean On Kill")
                .AddOnOffValues()
                .BindBool(b => cleanOnKill = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(Override.KillCooldown, () => cleanCooldown.Duration * 2, () => cleanCooldown.NotReady());

    private class FakeFatalIntent : IFatalIntent
    {
        public void Action(PlayerControl actor, PlayerControl target)
        {
        }

        public void Halted(PlayerControl actor, PlayerControl target)
        {
        }

        public Optional<IDeathEvent> CauseOfDeath() => Optional<IDeathEvent>.Null();

        public bool IsRanged() => false;
    }
}
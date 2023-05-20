using System.Linq;
using HarmonyLib;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Managers.History.Events;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Networking.RPC.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Janitor: Vanilla.Impostor
{
    private bool cleanOnKill;

    [UIComponent(UI.Cooldown)]
    private Cooldown cleanCooldown;

    [RoleAction(RoleActionType.Attack)]
    public new bool TryKill(PlayerControl target)
    {
        cleanCooldown.Start(AUSettings.KillCooldown());

        if (!cleanOnKill) return base.TryKill(target);

        if (MyPlayer.InteractWith(target, new DirectInteraction(new FakeFatalIntent(), this)) is InteractionResult.Halt) return false;
        MyPlayer.RpcMark(target);
        RpcV3.Standard(MyPlayer.NetId, RpcCalls.MurderPlayer).Write(target).Send(target.GetClientId());
        target.RpcExileV2();
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, target));
        Game.MatchData.GameHistory.AddEvent(new GenericAbilityEvent(MyPlayer, $"{Color.red.Colorize(MyPlayer.name)} cleaned {target.GetRoleColor().Colorize(target.name)}."));
        return true;
    }

    [RoleAction(RoleActionType.SelfReportBody)]
    private void JanitorCleanBody(GameData.PlayerInfo target, ActionHandle handle)
    {
        if (cleanCooldown.NotReady()) return;
        handle.Cancel();
        cleanCooldown.Start();

        byte playerId = target.Object.PlayerId;

        foreach (DeadBody deadBody in Object.FindObjectsOfType<DeadBody>())
            if (deadBody.ParentId == playerId)
                if (ModVersion.AllClientsModded()) CleanBody(playerId);
                else Game.MatchData.UnreportableBodies.Add(playerId);

        MyPlayer.RpcMark(MyPlayer);
    }

    [ModRPC(RoleRPC.RemoveBody, invocation: MethodInvocation.ExecuteAfter)]
    private static void CleanBody(byte playerId)
    {
        VentLogger.Debug("Destroying Bodies", "JanitorClean");
        Object.FindObjectsOfType<DeadBody>().ToArray().Where(db => db.ParentId == playerId).ForEach(b => Object.Destroy(b.gameObject));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Clean On Kill")
                .AddOnOffValues()
                .BindBool(b => cleanOnKill = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(new IndirectKillCooldown(KillCooldown, () => cleanOnKill || cleanCooldown.NotReady()));

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
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Gamemodes;
using Lotus.Managers.History.Events;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Extensions;
using Lotus.RPC;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Patches.Actions;


public static class MurderPatches
{

    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    public static bool Prefix(PlayerControl __instance, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        if (__instance == null || target == null) return false;

        VentLogger.Debug($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}", "CheckMurder");
        if (Game.CurrentGamemode.IgnoredActions().HasFlag(GameAction.KillPlayers)) return false;


        if (target.Data == null || target.inVent || target.inMovingPlat)
        {
            VentLogger.Trace($"Unable to kill {target.name}. Invalid Status", "CheckMurder");
            return false;
        }
        if (!target.IsAlive())
        {
            VentLogger.Trace($"Unable to kill {target.name}. Player is already dead.", "CheckMurder");
            return false;
        }
        if (MeetingHud.Instance != null)
        {
            VentLogger.Trace($"Unable to kill {target.name}. There is currently a meeting.", "CheckMurder");
            return false;
        }

        if (__instance.PlayerId == target.PlayerId) return false;

        ActionHandle handle = ActionHandle.NoInit();
        __instance.Trigger(RoleActionType.Attack, ref handle, target);
        return false;
    }

    [QuickPostfix(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static void MurderPlayer(PlayerControl __instance, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!target.Data.IsDead) return;

        VentLogger.Trace($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}{(target.protectedByGuardian ? "(Protected)" : "")}", "MurderPlayer");

        IDeathEvent deathEvent = Game.MatchData.GameHistory.GetCauseOfDeath(target.PlayerId)
            .OrElseGet(() => __instance.PlayerId == target.PlayerId
                ? new SuicideEvent(__instance)
                : new DeathEvent(target, __instance)
            );

        Game.MatchData.GameHistory.AddEvent(deathEvent);
        Game.MatchData.GameHistory.SetCauseOfDeath(target.PlayerId, deathEvent);


        ActionHandle ignored = ActionHandle.NoInit();
        target.Trigger(RoleActionType.MyDeath, ref ignored, __instance, deathEvent.Instigator(), deathEvent);
        Game.TriggerForAll(RoleActionType.AnyDeath, ref ignored, target, __instance, deathEvent.Instigator(), deathEvent);

        PlayerMurderHookEvent playerMurderHookEvent = new(__instance, target, deathEvent);
        Hooks.PlayerHooks.PlayerMurderHook.Propagate(playerMurderHookEvent);
        Hooks.PlayerHooks.PlayerDeathHook.Propagate(playerMurderHookEvent);

        Async.Schedule(() =>
        {
            if (target != null) target.SetChatName(target.name);
        }, 0.1f);
    }
}
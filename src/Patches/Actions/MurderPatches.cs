using System;
using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Managers.History.Events;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using Lotus.Utilities;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;
using VentLib.Utilities.Optionals;

namespace Lotus.Patches.Actions;


public static class MurderPatches
{
    public static PlayerControl LastAttacker = null!;
    private static readonly Dictionary<byte, FixedUpdateLock> MurderLocks = new();
    private static readonly Func<FixedUpdateLock> TimeoutSupplier = () => new FixedUpdateLock(0.25f);

    public static bool Lock(byte player) => MurderLocks.GetOrCompute(player, TimeoutSupplier).AcquireLock(NetUtils.DeriveDelay(0.25f));

    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    public static bool Prefix(PlayerControl __instance, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        if (__instance == null || target == null) return false;

        VentLogger.Debug($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}", "CheckMurder");


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

        if (!MurderLocks.GetOrCompute(__instance.PlayerId, TimeoutSupplier).IsUnlocked()) return false;

        ActionHandle handle = ActionHandle.NoInit();
        __instance.Trigger(LotusActionType.Attack, ref handle, target);
        return false;
    }

    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static void SaveAttacker(PlayerControl __instance)
    {
        LastAttacker = __instance;
    }

    [QuickPostfix(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static void MurderPlayer(PlayerControl __instance, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!target.Data.IsDead) return;
        Lock(__instance.PlayerId);

        VentLogger.Trace($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}{(target.protectedByGuardian ? "(Protected)" : "")}", "MurderPlayer");

        IDeathEvent deathEvent = Game.MatchData.GameHistory.GetCauseOfDeath(target.PlayerId)
            .OrElseGet(() => __instance.PlayerId == target.PlayerId
                ? new SuicideEvent(__instance)
                : new DeathEvent(target, __instance)
            );

        Game.MatchData.GameHistory.AddEvent(deathEvent);
        Game.MatchData.GameHistory.SetCauseOfDeath(target.PlayerId, deathEvent);


        ActionHandle ignored = ActionHandle.NoInit();
        target.Trigger(LotusActionType.MyDeath, ref ignored, __instance, deathEvent.Instigator(), deathEvent);
        Game.TriggerForAll(LotusActionType.AnyDeath, ref ignored, target, __instance, deathEvent.Instigator(), deathEvent);

        PlayerControl killer = deathEvent.Instigator().FlatMap(k => new UnityOptional<PlayerControl>(k.MyPlayer)).OrElse(__instance);
        PlayerMurderHookEvent playerMurderHookEvent = new(killer, target, deathEvent);
        Hooks.PlayerHooks.PlayerMurderHook.Propagate(playerMurderHookEvent);
        Hooks.PlayerHooks.PlayerDeathHook.Propagate(playerMurderHookEvent);

        Async.Schedule(() =>
        {
            if (target != null) target.SetChatName(target.name);
        }, 0.1f);
    }
}
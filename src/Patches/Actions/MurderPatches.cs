using System.Collections.Generic;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Extensions;
using TOHTOR.Gamemodes;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;

namespace TOHTOR.Patches.Actions;


public static class MurderPatches
{
    private static readonly HashSet<byte> DeferredDeaths = new();

    static MurderPatches()
    {
        Hooks.GameStateHooks.GameStartHook.Bind(nameof(MurderPatches), _ => DeferredDeaths.Clear());
    }

    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    public static bool Prefix(PlayerControl __instance, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        if (DeferredDeaths.Contains(target.PlayerId)) return false;
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
        // Needed because this patch does not guarantee a player is dead
        if (!target.Data.IsDead && !DeferredDeaths.Contains(target.PlayerId)) return;
        VentLogger.Trace($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}{(target.protectedByGuardian ? "(Protected)" : "")}", "MurderPlayer");

        IDeathEvent deathEvent = Game.MatchData.GameHistory.GetCauseOfDeath(target.PlayerId)
            .OrElseGet(() => __instance.PlayerId == target.PlayerId
                ? new SuicideEvent(__instance)
                : new DeathEvent(target, __instance)
            );

        Game.MatchData.GameHistory.AddEvent(deathEvent);
        Game.MatchData.GameHistory.SetCauseOfDeath(target.PlayerId, deathEvent);


        ActionHandle ignored = ActionHandle.NoInit();
        target.Trigger(RoleActionType.MyDeath, ref ignored, __instance, deathEvent.Instigator());
        Game.TriggerForAll(RoleActionType.AnyDeath, ref ignored, target, __instance, deathEvent.Instigator());

        Hooks.PlayerHooks.PlayerMurderHook.Propagate(new PlayerMurderHookEvent(__instance, target));
        Hooks.PlayerHooks.PlayerDeathHook.Propagate(new PlayerHookEvent(target));
    }

    [QuickPostfix(typeof(PlayerControl), nameof(PlayerControl.Die))]
    public static void DeferTargetDeath(PlayerControl __instance, DeathReason reason, bool assignGhostRole)
    {
        if (!AmongUsClient.Instance.AmHost || __instance.IsHost()) return;
        DeferredDeaths.Add(__instance.PlayerId);
        __instance.Data.IsDead = false;
        Async.Schedule(() => RealDie(__instance, reason, assignGhostRole), NetUtils.DeriveDelay(0.05f));
    }

    private static void RealDie(PlayerControl target, DeathReason reason, bool assignGhostRole)
    {
        VentLogger.Trace($"Updating Dead Player State for: {target.name}", "MurderPatches::RealDie");
        TempData.LastDeathReason = reason;
        target.cosmetics.AnimatePetMourning();
        target.Data.IsDead = true;
        target.gameObject.layer = LayerMask.NameToLayer("Ghost");
        target.cosmetics.SetNameMask(false);
        target.cosmetics.PettingHand.StopPetting();
        GameManager.Instance.OnPlayerDeath(target, assignGhostRole);
        if (!target.AmOwner)
            return;
        DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(true);
        target.AdjustLighting();
    }
}
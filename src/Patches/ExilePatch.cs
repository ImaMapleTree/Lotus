using System;
using System.Collections.Generic;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.API.Vanilla.Meetings;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Managers.History.Events;
using Lotus.Roles.Internals.Enums;
using VentLib.Logging;

namespace Lotus.Patches;

static class ExileControllerWrapUpPatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class BaseExileControllerPatch
    {
        public static void Postfix(ExileController __instance)
        {
            try
            {
                WrapUpPostfix(__instance.exiled);
            }
            finally {
                WrapUpFinalizer();
            }
        }
    }

    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    class AirshipExileControllerPatch
    {
        public static void Postfix(AirshipExileController __instance)
        {
            try {
                WrapUpPostfix(__instance.exiled);
            }
            finally {
                WrapUpFinalizer();
            }
        }
    }
    static void WrapUpPostfix(GameData.PlayerInfo? exiled)
    {
        if (!AmongUsClient.Instance.AmHost) return; //ホスト以外はこれ以降の処理を実行しません;
        FallFromLadder.Reset();

        if (exiled == null) return;

        ActionHandle selfExiledHandle = ActionHandle.NoInit();
        ActionHandle otherExiledHandle = ActionHandle.NoInit();

        exiled.Object!.Trigger(LotusActionType.SelfExiled, ref selfExiledHandle);
        Game.TriggerForAll(LotusActionType.AnyExiled, ref otherExiledHandle, exiled);

        Hooks.PlayerHooks.PlayerExiledHook.Propagate(new PlayerHookEvent(exiled.Object!));
        Hooks.PlayerHooks.PlayerDeathHook.Propagate(new PlayerDeathHookEvent(exiled.Object!, new ExiledEvent(exiled.Object!, new List<PlayerControl>(), new List<PlayerControl>())));
    }

    static void WrapUpFinalizer()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        try
        {
            MeetingDelegate.Instance.BlackscreenResolver.ClearBlackscreen(BeginRoundStart);
            VentLogger.Debug("Start Task Phase", "Phase");
        }
        catch
        {
            BeginRoundStart();
        }
    }

    /// <summary>
    /// Called after Clear Blackscreen is done processing
    /// </summary>
    private static void BeginRoundStart()
    {
        try
        {
            Game.RenderAllForAll(force: true);
        }
        catch (Exception exception)
        {
            VentLogger.Exception(exception);
        }

        Game.State = GameState.Roaming;
        ActionHandle handle = ActionHandle.NoInit();
        VentLogger.Debug("Triggering RoundStart Action!!", "Exile::BeginRoundStart");
        Game.TriggerForAll(LotusActionType.RoundStart, ref handle, false);
        Hooks.GameStateHooks.RoundStartHook.Propagate(new GameStateHookEvent(Game.MatchData));
        Game.SyncAll();
    }
}

[HarmonyPatch(typeof(PbExileController), nameof(PbExileController.PlayerSpin))]
class PolusExileHatFixPatch
{
    public static void Prefix(PbExileController __instance)
    {
        __instance.Player.cosmetics.hat.transform.localPosition = new(-0.2f, 0.6f, 1.1f);
    }
}
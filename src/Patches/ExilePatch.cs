using AmongUs.Data;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.API.Vanilla.Meetings;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Extensions;
using VentLib.Logging;
using VentLib.Utilities;

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

        exiled.Object!.Trigger(RoleActionType.SelfExiled, ref selfExiledHandle);
        Game.TriggerForAll(RoleActionType.AnyExiled, ref otherExiledHandle, exiled);

        Hooks.PlayerHooks.PlayerExiledHook.Propagate(new PlayerHookEvent(exiled.Object!));
        Hooks.PlayerHooks.PlayerDeathHook.Propagate(new PlayerDeathHookEvent(exiled.Object!, ModConstants.DeathNames.Exiled));
    }

    static void WrapUpFinalizer()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        MeetingDelegate.Instance.BlackscreenResolver.ClearBlackscreen();

        SoundManager.Instance.ChangeMusicVolume(DataManager.Settings.Audio.MusicVolume);
        VentLogger.Debug("Start Task Phase", "Phase");

        Game.State = GameState.Roaming;
        Game.RenderAllForAll(force: true);
        //if (GeneralOptions.GameplayOptions.ForceNoVenting) Game.GetAlivePlayers().Where(p => !p.GetCustomRole().BaseCanVent).ForEach(VentApi.ForceNoVenting);
        Async.Schedule(() =>
        {
            ActionHandle handle = ActionHandle.NoInit();
            Game.TriggerForAll(RoleActionType.RoundStart, ref handle, false);
            Hooks.GameStateHooks.RoundStartHook.Propagate(new GameStateHookEvent(Game.MatchData));
        }, 1f);



        /*Async.Schedule(() =>
        {
            VentLogger.Fatal("TESTING!!!!!!!!!");

            Game.GetAllPlayers().ForEach(p =>
            {
                p.RpcSetName("TEST TEST TEST!!");
                NameUpdateProcess.Paused = true;
            });

            VentLogger.Fatal("Set All Player NaMES");
        }, 8);*/
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
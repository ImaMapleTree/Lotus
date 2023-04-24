using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Extensions;
using TOHTOR.Managers;
using TOHTOR.Options;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Patches;

static class ExileControllerWrapUpPatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class BaseExileControllerPatch
    {
        public static void Postfix(ExileController __instance)
        {
            try {
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
        if (!AmongUsClient.Instance.AmHost) return; //ホスト以外はこれ以降の処理を実行しません
        exiled = AntiBlackout.ExiledPlayer ?? exiled;
        if (exiled != null)
        {
            //霊界用暗転バグ対処
            if (!AntiBlackout.OverrideExiledPlayer && TOHPlugin.ResetCamPlayerList.Contains(exiled.PlayerId))
                exiled.Object?.ResetPlayerCam(1f);


            ActionHandle selfExiledHandle = ActionHandle.NoInit();
            ActionHandle otherExiledHandle = ActionHandle.NoInit();
            GameData.PlayerInfo realExiled = AntiBlackout.ExiledPlayer ?? exiled;

            realExiled.Object.Trigger(RoleActionType.SelfExiled, ref selfExiledHandle);
            Game.TriggerForAll(RoleActionType.AnyExiled, ref otherExiledHandle, realExiled);

            Hooks.PlayerHooks.PlayerExiledHook.Propagate(new PlayerHookEvent(exiled.Object!));
            Hooks.PlayerHooks.PlayerDeathHook.Propagate(new PlayerHookEvent(exiled.Object!));
        }
        FallFromLadder.Reset();
    }

    static void WrapUpFinalizer()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        Async.Schedule(AntiblackOutRestore, NetUtils.DeriveDelay(0.8f));

        /*RemoveDisableDevicesPatch.UpdateDisableDevices();*/
        SoundManager.Instance.ChangeMusicVolume(DataManager.Settings.Audio.MusicVolume);
        VentLogger.Debug("Start Task Phase", "Phase");

        //AntiBlackout.LoadCosmetics();
        AntiBlackout.FakeExiled = null;

        Game.State = GameState.Roaming;
        if (GeneralOptions.GameplayOptions.ForceNoVenting) Game.GetAlivePlayers().Where(p => !p.GetCustomRole().BaseCanVent).ForEach(VentApi.ForceNoVenting);
        Async.Schedule(() =>
        {
            ActionHandle handle = ActionHandle.NoInit();
            Game.TriggerForAll(RoleActionType.RoundStart, ref handle, false);
            Hooks.GameStateHooks.RoundStartHook.Propagate(new GameStateHookEvent());
            Game.RenderAllForAll(force: true);
        }, 0.5f);

    }

    private static void AntiblackOutRestore()
    {
        GameData.PlayerInfo? exiled = AntiBlackout.ExiledPlayer;
        AntiBlackout.LoadCosmetics();
        AntiBlackout.RestoreIsDead(doSend: true);
        if (exiled?.Object == null) return;
        exiled.Object.RpcExileV2();
        Async.Schedule(() => exiled.Object.RpcExileV2(), NetUtils.DeriveDelay(0.8f));
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
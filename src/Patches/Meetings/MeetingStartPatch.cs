using HarmonyLib;
using TOHTOR.API;
using TOHTOR.API.Meetings;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Extensions;
using TOHTOR.Options;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Utilities;
using VentLib.Localization;
using VentLib.Logging;
using VentLib.Utilities;

namespace TOHTOR.Patches.Meetings;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public class MeetingStartPatch
{
    public static MeetingDelegate MeetingDelegate;

    public static void Prefix(MeetingHud __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        Game.State = GameState.InMeeting;
        VentLogger.Info("------------Meeting Start------------", "Phase");

        MeetingDelegate = new MeetingDelegate();

        Game.GetAlivePlayers().Do(p =>
        {
            if (Game.GameStates.MeetingCalled == 0 && TOHPlugin.PluginDataManager.TemplateManager.TryFormat(p, "meeting-first", out string msg))
                Utils.SendMessage(msg, p.PlayerId);

            if (TOHPlugin.PluginDataManager.TemplateManager.TryFormat(p, "meeting-start", out string message))
                Utils.SendMessage(message, p.PlayerId);
        });

        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.RoundEnd, ref handle, MeetingDelegate, false);
        Game.RenderAllForAll(force: true);

        Hooks.MeetingHooks.MeetingCalledHook.Propagate(new MeetingHookEvent(MeetingDelegate));

        Game.GameStates.MeetingCalled++;
        Async.Schedule(() => PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.UnalteredName()), NetUtils.DeriveDelay(0f));
        Async.Schedule(() => PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.UnalteredName()), NetUtils.DeriveDelay(0.1f));
        Async.Schedule(() => PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.UnalteredName()), NetUtils.DeriveDelay(0.2f));
        Async.Schedule(() => PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.UnalteredName()), NetUtils.DeriveDelay(0.3f));
        Async.Schedule(() => PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.UnalteredName()), NetUtils.DeriveDelay(0.4f));
        Async.Schedule(() => PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.UnalteredName()), NetUtils.DeriveDelay(0.5f));
        Async.Schedule(() => PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.UnalteredName()), NetUtils.DeriveDelay(0.6f));
        Async.Schedule(() => PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.UnalteredName()), NetUtils.DeriveDelay(0.7f));
    }

    public static void Postfix(MeetingHud __instance)
    {
        SoundManager.Instance.ChangeMusicVolume(0f);
        if (StaticOptions.SyncButtonMode)
        {
            Utils.SendMessage(string.Format(Localizer.Get("StaticOptions.SyncButton.SyncButtonsLeft"), StaticOptions.SyncedButtonCount - StaticOptions.UsedButtonCount));
            VentLogger.Old("緊急会議ボタンはあと" + (StaticOptions.SyncedButtonCount - StaticOptions.UsedButtonCount) + "回使用可能です。", "SyncButtonMode");
        }

        /*if (AmongUsClient.Instance.AmHost)
            Async.Schedule(() => ChatUpdatePatch.DoBlockChat = false, 3f);*/
    }
}
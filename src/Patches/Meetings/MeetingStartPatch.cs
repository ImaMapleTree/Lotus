using System;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.API.Vanilla.Meetings;
using Lotus.Chat;
using Lotus.Managers;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Utilities;
using VentLib.Logging;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;

namespace Lotus.Patches.Meetings;

[LoadStatic]
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public class MeetingStartPatch
{
    static MeetingStartPatch()
    {
        PluginDataManager.TemplateManager.RegisterTag("meeting-first", "Tag for the template to be shown during the first meeting.");
        PluginDataManager.TemplateManager.RegisterTag("meeting-start", "Tag for the template to be shown during each meeting.");
    }

    public static void Prefix(MeetingHud __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        VentLogger.Info("------------Meeting Start------------", "Phase");

        MeetingDelegate meetingDelegate = MeetingPrep.PrepMeeting();
        PlayerControl reporter = Utils.GetPlayerById(__instance.reporterId)!;

        try
        {
            Game.GetAlivePlayers().Do(p =>
            {
                if (Game.MatchData.MeetingsCalled == 0 &&
                    PluginDataManager.TemplateManager.TryFormat(p, "meeting-first", out string msg))
                    ChatHandler.Of(msg).LeftAlign().Send(p);

                if (PluginDataManager.TemplateManager.TryFormat(p, "meeting-start", out string message))
                    ChatHandler.Of(message).LeftAlign().Send(p);
            });
        } catch (Exception ex) { VentLogger.Exception(ex, "Error Sending Template Information!"); }

        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.RoundEnd, ref handle, meetingDelegate, false);
        Game.RenderAllForAll(force: true);

        Hooks.MeetingHooks.MeetingCalledHook.Propagate(new MeetingHookEvent(reporter, MeetingPrep.Reported, meetingDelegate));
        Game.MatchData.MeetingsCalled++;
    }

    public static void Postfix(MeetingHud __instance)
    {
        SoundManager.Instance.ChangeMusicVolume(0f);
        if (AmongUsClient.Instance.AmHost) __instance.playerStates.ToArray()
            .FirstOrOptional(ps => ps.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId)
            .IfPresent(voteArea => voteArea.NameText.text = PlayerControl.LocalPlayer.NameModel().Render(sendToPlayer: false, force: true));

        // TODO: Sync Button Mode

        /*if (StaticOptions.SyncButtonMode)
        {
            Utils.SendMessage(string.Format(Localizer.Translate("StaticOptions.SyncButton.SyncButtonsLeft"), StaticOptions.SyncedButtonCount - StaticOptions.UsedButtonCount));
            VentLogger.Old("緊急会議ボタンはあと" + (StaticOptions.SyncedButtonCount - StaticOptions.UsedButtonCount) + "回使用可能です。", "SyncButtonMode");
        }*/

        /*if (AmongUsClient.Instance.AmHost)
            Async.Schedule(() => ChatUpdatePatch.DoBlockChat = false, 3f);*/
    }
}
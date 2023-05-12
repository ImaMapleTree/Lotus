using System.Linq;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.API.Vanilla.Meetings;
using TOHTOR.Extensions;
using TOHTOR.Managers;
using TOHTOR.Options;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Utilities;
using VentLib.Localization;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Patches.Meetings;

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

        Game.GetAlivePlayers().Do(p =>
        {
            if (Game.MatchData.MeetingsCalled == 0 && PluginDataManager.TemplateManager.TryFormat(p, "meeting-first", out string msg))
                Utils.SendMessage(msg, p.PlayerId);

            if (PluginDataManager.TemplateManager.TryFormat(p, "meeting-start", out string message))
                Utils.SendMessage(message, p.PlayerId);
        });


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
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Meetings;
using Lotus.Gamemodes;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.API;
using Lotus.Extensions;
using Lotus.Patches.Meetings;
using Lotus.Utilities;
using VentLib.Logging;
using VentLib.Utilities;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
public class ReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
    {
        VentLogger.Old($"{__instance.GetNameWithRole()} => {target?.Object?.GetNameWithRole() ?? "null"}", "ReportDeadBody");
        if (Game.CurrentGamemode.IgnoredActions().HasFlag(GameAction.ReportBody) && target != null) return false;
        if (Game.CurrentGamemode.IgnoredActions().HasFlag(GameAction.CallMeeting) && target == null) return false;
        if (!AmongUsClient.Instance.AmHost) return true;
        
        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.MeetingCalled, ref handle, __instance);

        if (handle.IsCanceled || target == null) return true;
        if (Game.MatchData.UnreportableBodies.Contains(target.PlayerId)) return false;

        __instance.Trigger(RoleActionType.SelfReportBody, ref handle, target);
        if (handle.IsCanceled) return false;
        Game.TriggerForAll(RoleActionType.AnyReportedBody, ref handle, __instance, target);
        if (handle.IsCanceled) return false;

        MeetingPrep.Reported = target;
        MeetingPrep.PrepMeeting(__instance);
        return false;
    }
}

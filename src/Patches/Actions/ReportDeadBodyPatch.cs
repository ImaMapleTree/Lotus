using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Meetings;
using Lotus.Gamemodes;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Extensions;
using Lotus.Utilities;
using VentLib.Logging;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
public class ReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo? target)
    {
        VentLogger.Trace($"{__instance.GetNameWithRole()} => {target?.GetNameWithRole() ?? "null"}", "ReportDeadBody");
        if (!AmongUsClient.Instance.AmHost) return true;

        if (Game.CurrentGamemode.IgnoredActions().HasFlag(GameAction.ReportBody) && target != null) return false;
        if (Game.CurrentGamemode.IgnoredActions().HasFlag(GameAction.CallMeeting) && target == null) return false;


        ActionHandle handle = ActionHandle.NoInit();

        if (target != null)
        {
            if (__instance.PlayerId == target.PlayerId) return false;
            if (Game.MatchData.UnreportableBodies.Contains(target.PlayerId)) return false;

            Game.TriggerForAll(RoleActionType.AnyReportedBody, ref handle, __instance, target);
            if (handle.IsCanceled)
            {
                VentLogger.Trace("Not Reporting Body - Cancelled by Any Report Action", "ReportDeadBody");
                return false;
            }
            
            __instance.Trigger(RoleActionType.SelfReportBody, ref handle, target);
            if (handle.IsCanceled)
            {
                VentLogger.Trace("Not Reporting Body - Cancelled by Self Report Action", "ReportDeadBody");
                return false;
            }
        }

        MeetingPrep.Reported = target;
        MeetingPrep.PrepMeeting(__instance);
        return false;
    }
}

using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Meetings;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles2.Operations;
using Lotus.Utilities;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
public class ReportDeadBodyPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(ReportDeadBodyPatch));

    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo? target)
    {
        log.Trace($"{__instance.GetNameWithRole()} => {target?.GetNameWithRole() ?? "null"}", "ReportDeadBody");
        if (!AmongUsClient.Instance.AmHost) return true;
        if (__instance.Data.IsDead) return false;


        ActionHandle handle = ActionHandle.NoInit();

        if (target != null)
        {
            if (__instance.PlayerId == target.PlayerId) return false;
            if (Game.MatchData.UnreportableBodies.Contains(target.PlayerId)) return false;

            RoleOperations.Current.Trigger(LotusActionType.ReportBody, __instance, handle, target);
            if (handle.IsCanceled)
            {
                log.Trace("Not Reporting Body - Cancelled by Any Report Action", "ReportDeadBody");
                return false;
            }
        }

        MeetingPrep.Reported = target;
        MeetingPrep.PrepMeeting(__instance, target);
        return false;
    }
}

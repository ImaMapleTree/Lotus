using HarmonyLib;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Vanilla.Meetings;
using TOHTOR.Extensions;
using TOHTOR.Gamemodes;
using TOHTOR.Patches.Meetings;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Utilities;
using VentLib.Logging;
using VentLib.Utilities;

namespace TOHTOR.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
public class ReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
    {
        VentLogger.Old($"{__instance.GetNameWithRole()} => {target?.Object?.GetNameWithRole() ?? "null"}", "ReportDeadBody");
        if (Game.CurrentGamemode.IgnoredActions().HasFlag(GameAction.ReportBody) && target != null) return false;
        if (Game.CurrentGamemode.IgnoredActions().HasFlag(GameAction.CallMeeting) && target == null) return false;
        if (!AmongUsClient.Instance.AmHost) return true;
        if (target == null) return true;
        if (Game.GameStates.UnreportableBodies.Contains(target.PlayerId)) return false;

        ActionHandle handle = ActionHandle.NoInit();
        __instance.Trigger(RoleActionType.SelfReportBody, ref handle, target);
        if (handle.IsCanceled) return false;
        Game.TriggerForAll(RoleActionType.AnyReportedBody, ref handle, __instance, target);
        if (handle.IsCanceled) return false;

        MeetingPrep.Reported = target;
        MeetingPrep.PrepMeeting(__instance);
        return false;
    }
}

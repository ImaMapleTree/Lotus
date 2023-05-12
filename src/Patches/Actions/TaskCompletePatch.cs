using HarmonyLib;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Logging;

namespace TOHTOR.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
class TaskCompletePatch
{
    public static void Prefix(PlayerControl __instance, uint idx)
    {
        VentLogger.Info($"TaskComplete:{__instance.GetNameWithRole()}", "CompleteTask");

        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.TaskComplete, ref handle, __instance, idx);

    }
}
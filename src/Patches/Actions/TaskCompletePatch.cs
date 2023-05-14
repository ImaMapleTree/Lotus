using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Extensions;
using VentLib.Logging;

namespace Lotus.Patches.Actions;

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
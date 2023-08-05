using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using VentLib.Utilities.Optionals;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
class TaskCompletePatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(TaskCompletePatch));

    public static void Prefix(PlayerControl __instance, uint idx)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (__instance == null) return;
        GameData.TaskInfo taskInfo = __instance.Data.FindTaskById(idx);
        NormalPlayerTask? npt = taskInfo == null! ? null : ShipStatus.Instance.GetTaskById(taskInfo!.TypeId);
        log.Info($"Task Complete => {__instance.GetNameWithRole()} ({npt?.Length})", "CompleteTask");

        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(LotusActionType.TaskComplete, ref handle, __instance, Optional<NormalPlayerTask>.Of(npt));
        Hooks.PlayerHooks.PlayerTaskCompleteHook.Propagate(new PlayerTaskHookEvent(__instance, npt));
    }
}
using System.Collections.Generic;
using HarmonyLib;
using TOHTOR.Options;
using TOHTOR.Options.General;
using VentLib.Logging;

namespace TOHTOR.Patches.Systems;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.AddTasksFromList))]
class AddTasksFromListPatch
{
    public static void Prefix(ShipStatus __instance, [HarmonyArgument(4)] Il2CppSystem.Collections.Generic.List<NormalPlayerTask> unusedTasks)
    {
        if (!GeneralOptions.GameplayOptions.DisableTasks) return;
        List<NormalPlayerTask> disabledTasks = new();
        for (int i = 0; i < unusedTasks.Count; i++)
        {
            var task = unusedTasks[i];
            switch (task.TaskType)
            {
                case TaskTypes.SwipeCard when GeneralOptions.GameplayOptions.DisabledTaskFlag.HasFlag(DisabledTask.CardSwipe):
                case TaskTypes.SubmitScan when GeneralOptions.GameplayOptions.DisabledTaskFlag.HasFlag(DisabledTask.MedScan):
                case TaskTypes.UnlockSafe when GeneralOptions.GameplayOptions.DisabledTaskFlag.HasFlag(DisabledTask.UnlockSafe):
                case TaskTypes.UploadData when GeneralOptions.GameplayOptions.DisabledTaskFlag.HasFlag(DisabledTask.UploadData):
                case TaskTypes.StartReactor when GeneralOptions.GameplayOptions.DisabledTaskFlag.HasFlag(DisabledTask.StartReactor):
                case TaskTypes.ResetBreakers when GeneralOptions.GameplayOptions.DisabledTaskFlag.HasFlag(DisabledTask.ResetBreaker):
                    disabledTasks.Add(task);
                    break;
            }
        }
        foreach (var task in disabledTasks)
        {
            VentLogger.Debug("Disabling Task: " + task.TaskType, "DisableTasks");
            unusedTasks.Remove(task);
        }
    }
}
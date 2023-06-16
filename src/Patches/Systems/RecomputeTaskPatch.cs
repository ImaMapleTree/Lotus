using System;
using System.Linq;
using HarmonyLib;
using Lotus.Extensions;
using Lotus.Roles.Interfaces;
using Lotus.Utilities;
using Lotus.Roles.Legacy;
using VentLib.Utilities.Extensions;

namespace Lotus.Patches.Systems;

[HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
public class RecomputeTaskPatch
{
    public static bool Prefix(GameData __instance)
    {
        __instance.TotalTasks = 0;
        __instance.CompletedTasks = 0;
        __instance.AllPlayers.ToArray()
            .Where(Utils.HasTasks)
            .Where(p => p.GetCustomRole() is ITaskHolderRole taskHolder && taskHolder.TasksApplyToTotal())
            .SelectMany(p => p?.Tasks?.ToArray() ?? Array.Empty<GameData.TaskInfo>())
            .ForEach(task =>
            {
                __instance.TotalTasks++;
                if (task.Complete) __instance.CompletedTasks++;
            });

        return false;
    }
}
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Lotus.API;
using Lotus.Roles;
using Lotus.Roles.Interfaces;
using Lotus.Utilities;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Roles2;

namespace Lotus.Patches.Systems;

[HarmonyPatch(typeof(GameData), nameof(GameData.RpcSetTasks))]
public class RpcSetTasksPatch
{
    internal static readonly Queue<TasksOverride> TaskQueue = new();

    public static bool Prefix(GameData __instance, byte playerId, ref Il2CppStructArray<byte> taskTypeIds)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        UnifiedRoleDefinition? role = Utils.GetPlayerById(playerId)?.PrimaryRole();
        // This function mostly deals with override, so if not overriding immediately exit

        TasksOverride? tasksOverride = TaskQueue.Count == 0 ? null : TaskQueue.Dequeue();

        int shortTaskCount = -1;
        int longTaskCount = -1;
        bool overrideTasks = false;
        bool hasCommonTasks = false;

        TaskContainer taskContainer = role?.Metadata.GetOrDefault(TaskContainer.Key, TaskContainer.None) ?? TaskContainer.None;
        if (!taskContainer.HasTasks) return true;

        if (taskContainer.TasksOverrideDefaults)
        {
            hasCommonTasks = taskContainer.HasCommonTasks;
            shortTaskCount = taskContainer.ShortTasks;
            longTaskCount = taskContainer.LongTasks;
            overrideTasks = true;
        }


        if (shortTaskCount == -1 || !overrideTasks) shortTaskCount = AUSettings.NumShortTasks();
        if (longTaskCount == -1 || !overrideTasks) longTaskCount = AUSettings.NumLongTasks();


        if (tasksOverride != null)
        {
            if (tasksOverride.ShortTasks == -1) tasksOverride.ShortTasks = shortTaskCount;
            if (tasksOverride.LongTasks == -1) tasksOverride.LongTasks = longTaskCount;
            if (tasksOverride.TaskAssignmentMode is TaskAssignmentMode.Add)
            {
                shortTaskCount += tasksOverride.ShortTasks;
                longTaskCount += tasksOverride.LongTasks;
            }
            else
            {
                shortTaskCount = tasksOverride.ShortTasks;
                longTaskCount = tasksOverride.LongTasks;
            }
        }
        else if (!overrideTasks) return true;

        Il2CppSystem.Collections.Generic.List<byte> tasksList = new();
        foreach (byte num in taskTypeIds) tasksList.Add(num);

        if (hasCommonTasks) tasksList.RemoveRange(AUSettings.NumCommonTasks(), tasksList.Count - AUSettings.NumCommonTasks());
        else tasksList.Clear();

        Il2CppSystem.Collections.Generic.HashSet<TaskTypes> usedTaskTypes = new();

        Il2CppSystem.Collections.Generic.List<NormalPlayerTask> longTasks = new();
        foreach (var task in ShipStatus.Instance.LongTasks)
            longTasks.Add(task);
        Shuffle(longTasks);

        Il2CppSystem.Collections.Generic.List<NormalPlayerTask> shortTasks = new();
        foreach (var task in ShipStatus.Instance.NormalTasks)
            shortTasks.Add(task);
        Shuffle(shortTasks);

        int start2 = 0;
        ShipStatus.Instance.AddTasksFromList(
            ref start2,
            longTaskCount,
            tasksList,
            usedTaskTypes,
            longTasks
        );

        int start3 = 0;
        ShipStatus.Instance.AddTasksFromList(
            ref start3,
            !hasCommonTasks && shortTaskCount == 0 && longTaskCount == 0 ? 1 : shortTaskCount,
            tasksList,
            usedTaskTypes,
            shortTasks
        );

        taskTypeIds = new Il2CppStructArray<byte>(tasksList.Count);
        for (int i = 0; i < tasksList.Count; i++) taskTypeIds[i] = tasksList[i];
        // If tasks apply to total then we're good, otherwise do our custom sending
        return true;
    }


    private static void WriteTaskArray(Il2CppStructArray<byte> taskIds, MessageWriter writer) => writer.WriteBytesAndSize(taskIds);

    public static void Shuffle<T>(Il2CppSystem.Collections.Generic.List<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            T obj = list[i];
            int rand = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[rand];
            list[rand] = obj;
        }
    }

    public class TasksOverride
    {
        public int ShortTasks;
        public int LongTasks;
        public TaskAssignmentMode TaskAssignmentMode;

        public TasksOverride(int shortTasks, int longTasks, TaskAssignmentMode taskAssignmentMode)
        {
            ShortTasks = shortTasks;
            LongTasks = longTasks;
            TaskAssignmentMode = taskAssignmentMode;
        }
    }
}
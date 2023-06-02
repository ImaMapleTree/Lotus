using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Lotus.Roles;
using VentLib.Logging;
using VentLib.Utilities;
using static Lotus.Patches.Systems.RpcSetTasksPatch;

namespace Lotus.API;

public class Tasks
{
    public static void AssignAdditionalTasks<T>(T taskHolder, int shortTasks = -1, int longTasks = -1, TaskAssignmentMode taskAssignmentMode = TaskAssignmentMode.Override, bool delayed = true, Action<TasksOverride>? callback = null) where T : CustomRole
    {
        if (!AmongUsClient.Instance.AmHost) return;
        Async.Schedule(() =>
        {
            VentLogger.Debug($"Assigning player: {taskHolder.MyPlayer.name} new tasks (Short={shortTasks}, Long={longTasks})", "AssignNewTasks");
            TasksOverride tasksOverride = new(shortTasks, longTasks, taskAssignmentMode);
            TaskQueue.Enqueue(tasksOverride);
            GameData.Instance.RpcSetTasks(taskHolder.MyPlayer.PlayerId, new Il2CppStructArray<byte>(0));
            callback?.Invoke(tasksOverride);
        }, delayed ? NetUtils.DeriveDelay(1f) : 0);
    }
}

public enum TaskAssignmentMode
{
    Add,
    Override
}
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Lotus.Patches.Systems;
using Lotus.Roles;
using VentLib.Logging;
using VentLib.Utilities;

namespace Lotus.API;

public class Tasks
{
    public static void AssignAdditionalTasks<T>(T taskHolder, int shortTasks = -1, int longTasks = -1, TaskAssignmentMode taskAssignmentMode = TaskAssignmentMode.Override, bool delayed = true) where T : CustomRole
    {
        Async.Schedule(() =>
        {
            VentLogger.Debug($"Assigning player: {taskHolder.MyPlayer.name} new tasks (Short={shortTasks}, Long={longTasks})", "AssignNewTasks");
            RpcSetTasksPatch.TaskQueue.Enqueue(new RpcSetTasksPatch.TasksOverride(shortTasks, longTasks, taskAssignmentMode));
            GameData.Instance.RpcSetTasks(taskHolder.MyPlayer.PlayerId, new Il2CppStructArray<byte>(0));
        }, delayed ? NetUtils.DeriveDelay(1f) : 0);
    }
}

public enum TaskAssignmentMode
{
    Add,
    Override
}
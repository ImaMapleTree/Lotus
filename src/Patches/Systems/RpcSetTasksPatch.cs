using System;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Lotus.API;
using Lotus.Roles;
using Lotus.Roles.Interfaces;
using Lotus.Utilities;
using Lotus.Extensions;
using VentLib.Networking.RPC;
using VentLib.Networking.RPC.Interfaces;
using VentLib.Utilities;

namespace Lotus.Patches.Systems;

[HarmonyPatch(typeof(GameData), nameof(GameData.RpcSetTasks))]
class RpcSetTasksPatch
{
    public static bool Prefix(GameData __instance, byte playerId, ref Il2CppStructArray<byte> taskTypeIds)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        CustomRole? role = Utils.GetPlayerById(playerId)?.GetCustomRole();
        // This function mostly deals with override, so if not overriding immediately exit
        if (role is not IOverridenTaskHolderRole tasksRole || !tasksRole.OverrideTasks())
        {
            // If not a task holder we don't care so just return true
            if (role is not ITaskHolderRole taskHolder) return true;
            // Otherwise, if tasks apply to total then we're gucci, so return true, else hide tasks to other players
            return taskHolder.TasksApplyToTotal() || SendNonCrewmateTasks(__instance, playerId, taskTypeIds);
        }

        bool hasCommonTasks = tasksRole.AssignCommonTasks(); // コモンタスク(通常タスク)を割り当てるかどうか
        // 割り当てる場合でも再割り当てはされず、他のクルーと同じコモンタスクが割り当てられる。


        Il2CppSystem.Collections.Generic.List<byte> tasksList = new();
        foreach (var num in taskTypeIds) tasksList.Add(num);

        if (hasCommonTasks) tasksList.RemoveRange(AUSettings.NumCommonTasks(), tasksList.Count - AUSettings.NumCommonTasks());
        else tasksList.Clear();

        Il2CppSystem.Collections.Generic.HashSet<TaskTypes> usedTaskTypes = new();
        int start2 = 0;
        int start3 = 0;

        Il2CppSystem.Collections.Generic.List<NormalPlayerTask> longTasks = new();
        if (longTasks == null) throw new ArgumentNullException(nameof(longTasks));
        foreach (var task in ShipStatus.Instance.LongTasks)
            longTasks.Add(task);
        Shuffle(longTasks);

        Il2CppSystem.Collections.Generic.List<NormalPlayerTask> shortTasks = new();
        foreach (var task in ShipStatus.Instance.NormalTasks)
            shortTasks.Add(task);
        Shuffle(shortTasks);

        ShipStatus.Instance.AddTasksFromList(
            ref start2,
            tasksRole.LongTaskAmount(),
            tasksList,
            usedTaskTypes,
            longTasks
        );
        ShipStatus.Instance.AddTasksFromList(
            ref start3,
            !hasCommonTasks && tasksRole.ShortTaskAmount() == 0 && tasksRole.LongTaskAmount() == 0 ? 1 : tasksRole.ShortTaskAmount(),
            tasksList,
            usedTaskTypes,
            shortTasks
        );

        taskTypeIds = new Il2CppStructArray<byte>(tasksList.Count);
        for (int i = 0; i < tasksList.Count; i++) taskTypeIds[i] = tasksList[i];
        // If tasks apply to total then we're good, otherwise do our custom sending
        return tasksRole.TasksApplyToTotal() || SendNonCrewmateTasks(__instance, playerId, taskTypeIds);
    }

    private static bool SendNonCrewmateTasks(GameData data, byte playerId, Il2CppStructArray<byte> taskTypeIds)
    {
        return true;
        int clientId = Utils.PlayerById(playerId).Map(p => p.GetClientId()).OrElse(-1);
        RpcV3.Immediate(data.NetId, RpcCalls.SetTasks).Write(playerId).WriteCustom(taskTypeIds, WriteTaskArray).Send(clientId);

        if (clientId == -1) return false;

        Il2CppStructArray<byte> noTasks = new(0);
        RpcV3.Immediate(data.NetId, RpcCalls.SetTasks).Write(playerId).WriteCustom(noTasks, WriteTaskArray).SendExcluding(clientId);
        return false;
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
}
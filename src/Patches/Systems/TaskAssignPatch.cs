using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Options;
using TOHTOR.Options.General;
using TOHTOR.Roles;
using TOHTOR.Roles.Interfaces;
using TOHTOR.Utilities;
using VentLib.Logging;

namespace TOHTOR.Patches.Systems;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.AddTasksFromList))]
class AddTasksFromListPatch
{
    public static void Prefix(ShipStatus __instance, [HarmonyArgument(4)] Il2CppSystem.Collections.Generic.List<NormalPlayerTask> unusedTasks)
    {
        if (!GeneralOptions.GameplayOptions.DisableTasks) return;
        List<NormalPlayerTask> disabledTasks = new();
        for (var i = 0; i < unusedTasks.Count; i++)
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
                    disabledTasks.Add(task);//カードタスク
                    break;
            }
        }
        foreach (var task in disabledTasks)
        {
            VentLogger.Info("削除: " + task.TaskType, "AddTask");
            unusedTasks.Remove(task);
        }
    }
}

[HarmonyPatch(typeof(GameData), nameof(GameData.RpcSetTasks))]
class RpcSetTasksPatch
{
    //タスクを割り当ててRPCを送る処理が行われる直前にタスクを上書きするPatch
    //バニラのタスク割り当て処理自体には干渉しない
    public static void Prefix(GameData __instance,
        [HarmonyArgument(0)] byte playerId,
        [HarmonyArgument(1)] ref Il2CppStructArray<byte> taskTypeIds)
    {

        CustomRole? role = Utils.GetPlayerById(playerId)?.GetCustomRole();
        if (role is not IOverridenTaskHolderRole tasksRole || !tasksRole.OverrideTasks()) return;

        bool hasCommonTasks = tasksRole.AssignCommonTasks(); // コモンタスク(通常タスク)を割り当てるかどうか
                                                                // 割り当てる場合でも再割り当てはされず、他のクルーと同じコモンタスクが割り当てられる。

        //本来のRpcSetTasksの第二引数のクローン
        Il2CppSystem.Collections.Generic.List<byte> tasksList = new();
        foreach (var num in taskTypeIds) tasksList.Add(num);

        //参考:ShipStatus.Begin
        //不要な割り当て済みのタスクを削除する処理
        //コモンタスクを割り当てる設定ならコモンタスク以外を削除
        //コモンタスクを割り当てない設定ならリストを空にする
        if (hasCommonTasks) tasksList.RemoveRange(AUSettings.NumCommonTasks(), tasksList.Count - AUSettings.NumCommonTasks());
        else tasksList.Clear();

        //割り当て済みのタスクが入れられるHashSet
        //同じタスクが複数割り当てられるのを防ぐ
        Il2CppSystem.Collections.Generic.HashSet<TaskTypes> usedTaskTypes = new();
        int start2 = 0;
        int start3 = 0;

        //割り当て可能なロングタスクのリスト
        Il2CppSystem.Collections.Generic.List<NormalPlayerTask> longTasks = new();
        if (longTasks == null) throw new ArgumentNullException(nameof(longTasks));
        foreach (var task in ShipStatus.Instance.LongTasks)
            longTasks.Add(task);
        Shuffle(longTasks);

        //割り当て可能なショートタスクのリスト
        Il2CppSystem.Collections.Generic.List<NormalPlayerTask> shortTasks = new();
        foreach (var task in ShipStatus.Instance.NormalTasks)
            shortTasks.Add(task);
        Shuffle(shortTasks);

        //実際にAmong Us側で使われているタスクを割り当てる関数を使う。
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

        //タスクのリストを配列(Il2CppStructArray)に変換する
        taskTypeIds = new Il2CppStructArray<byte>(tasksList.Count);
        for (int i = 0; i < tasksList.Count; i++)
        {
            taskTypeIds[i] = tasksList[i];
        }

    }
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
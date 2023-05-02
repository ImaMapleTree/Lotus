using System;
using HarmonyLib;
using TOHTOR.Extensions;
using TOHTOR.Roles;
using TOHTOR.Roles.Legacy;
using VentLib.Utilities;

namespace TOHTOR.GUI.Hud;

[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
class TaskTextPatch
{
    // タスク表示の文章が更新・適用された後に実行される
    public static void Postfix(TaskPanelBehaviour __instance)
    {
        PlayerControl player = PlayerControl.LocalPlayer;
        CustomRole role = player.GetCustomRole();
        if (role.IsVanilla()) return;

        string modifiedText = __instance.taskText.text;
        int impostorTaskIndex = modifiedText.IndexOf(":</color>", StringComparison.Ordinal);
        if (impostorTaskIndex != -1) modifiedText = modifiedText[(9 + impostorTaskIndex)..];
        string roleWithInfo = $"{role.RoleName}:\r\n";
        roleWithInfo += role.Blurb + (role.RealRole.IsImpostor() ? "" : "\r\n");
        __instance.taskText.text = role.RoleColor.Colorize(roleWithInfo) + modifiedText;
    }
}
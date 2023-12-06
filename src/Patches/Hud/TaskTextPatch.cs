using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.Extensions;
using Lotus.Roles;
using Lotus.Roles2;
using Lotus.Victory;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Patches.Hud;

[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
class TaskTextPatch
{
    private static UnifiedRoleDefinition _role = null!;
    private static List<UnifiedRoleDefinition> _subroles = null!;
    private static DateTime _undeferredCheck = DateTime.Now;

    private static string roleText = "";
    private static string subroleText = "";
    private static int subroleCount = 0;

    // タスク表示の文章が更新・適用された後に実行される
    public static void Postfix(TaskPanelBehaviour __instance)
    {
        if (LobbyBehaviour.Instance != null) return;
        PlayerControl player = PlayerControl.LocalPlayer;

        if (CheckEndGamePatch.Deferred) _undeferredCheck = DateTime.Now;

        if (DateTime.Now.Subtract(_undeferredCheck).TotalSeconds > 2)
        {
            UnifiedRoleDefinition role = player.PrimaryRole();
            if (!ReferenceEquals(role, _role)) roleText = $"{role.Name}:\n{role.Blurb}";
            _role = role;
            List<UnifiedRoleDefinition> srs = player.SecondaryRoles();
            if (srs.Count != subroleCount || _subroles != srs) subroleText = srs.Select(sr => sr.ColoredRoleName()).Fuse();
            subroleCount = srs.Count;
            _subroles = srs;
        }

        if (ReferenceEquals(_role, null)) return;



        string modifiedText = __instance.taskText.text;
        int impostorTaskIndex = modifiedText.IndexOf(":</color>", StringComparison.Ordinal);
        if (impostorTaskIndex != -1) modifiedText = modifiedText[(9 + impostorTaskIndex)..];
        string finalText = roleText;
        if (subroleText != "") finalText += "\n" + subroleText;
        finalText += _role.RoleDefinition.Role.IsImpostor() ? "" : "\r\n";

        __instance.taskText.text = _role.RoleColor.Colorize(finalText) + modifiedText;
    }
}
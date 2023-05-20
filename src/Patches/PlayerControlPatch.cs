using HarmonyLib;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Logging;

namespace Lotus.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckProtect))]
class CheckProtectPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        VentLogger.Trace($"Check Protect: {__instance.GetNameWithRole()} => {target.GetNameWithRole()}", "CheckProtect");
        return true;
    }
}


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
class PlayerStartPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        __instance.cosmetics.colorBlindText.transform.localPosition += new Vector3(0f, -1.3f);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RemoveProtection))]
class PlayerControlRemoveProtectionPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        VentLogger.Old($"{__instance.GetNameWithRole()}", "RemoveProtection");
    }
}
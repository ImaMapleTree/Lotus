using System;
using HarmonyLib;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Logging;
using Object = UnityEngine.Object;

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

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ResetForMeeting))]
public static class ResetForMeetingPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        try
        {
            if (__instance.cosmetics.CurrentPet != null)
                __instance.cosmetics.CurrentPet.SetGettingPet(false,
                    __instance.cosmetics.CurrentPet.transform.position);
            if (!__instance.GetComponent<DummyBehaviour>().enabled)
            {
                __instance.MyPhysics.ExitAllVents();
                ShipStatus.Instance.SpawnPlayer(__instance, GameData.Instance.PlayerCount, false);
            }

            __instance.RemoveProtection();
            __instance.NetTransform.enabled = true;
            __instance.MyPhysics.ResetMoveState();
            for (int index = 0; index < __instance.currentRoleAnimations.Count; ++index)
            {
                if ((Object)__instance.currentRoleAnimations[index] != null &&
                    __instance.currentRoleAnimations[index].gameObject != null)
                {
                    Object.Destroy(__instance.currentRoleAnimations[index].gameObject);
                    __instance.logger.Error("Encountered a null Role Animation while destroying.");
                }
            }

            __instance.currentRoleAnimations.Clear();
        }
        catch (Exception exception)
        {
            VentLogger.Exception(exception);
        }

        return false;
    }
}
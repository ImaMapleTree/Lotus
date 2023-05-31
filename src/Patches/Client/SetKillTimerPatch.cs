using System;
using HarmonyLib;
using Lotus.API;

namespace Lotus.Patches.Client;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public class SetKillTimerPatch
{
    public static bool Prefix(PlayerControl __instance, float time)
    {
        __instance.killTimer = Math.Max(time, 0);
        DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(__instance.killTimer, Math.Max(AUSettings.KillCooldown(), __instance.killTimer));
        return false;
    }
}
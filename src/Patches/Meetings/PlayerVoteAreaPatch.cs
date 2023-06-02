using HarmonyLib;
using UnityEngine;

namespace Lotus.Patches.Meetings;

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Start))]
public class PlayerVoteAreaPatch
{
    public static void Postfix(PlayerVoteArea __instance)
    {
        if (__instance == null || __instance.ColorBlindName == null) return;
        __instance.ColorBlindName.transform.localPosition -= new Vector3(1.25f, 0.15f);
    }
}
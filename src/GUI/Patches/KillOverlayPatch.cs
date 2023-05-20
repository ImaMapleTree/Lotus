using System;
using HarmonyLib;
using Lotus.Logging;

namespace Lotus.GUI.Patches;

[HarmonyPatch(typeof(KillOverlay), nameof(KillOverlay.ShowKillAnimation))]
public class KillOverlayPatch
{
    private static DateTime _lastOverlay = DateTime.Now;

    public static bool Prefix(KillOverlay __instance)
    {
        DevLogger.Log("Showing Kill Animation");
        bool show = (DateTime.Now - _lastOverlay).TotalSeconds > 0.5f;
        _lastOverlay = DateTime.Now;
        return show;
    }
}
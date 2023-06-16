using HarmonyLib;
using Lotus.Options;

namespace Lotus.Patches.Systems;

[HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.Detoriorate))]
public static class DeteriorateCrashCoursePatch
{
    public static void Prefix(HeliSabotageSystem __instance)
    {
        if (!__instance.IsActive) return;

        if (!GeneralOptions.SabotageOptions.CustomAirshipReactorCountdown) return;

        if (__instance.Countdown > GeneralOptions.SabotageOptions.AirshipReactorCountdown)
            __instance.Countdown = GeneralOptions.SabotageOptions.AirshipReactorCountdown;
    }
}
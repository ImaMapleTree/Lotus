using HarmonyLib;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Options;

namespace Lotus.Patches.Systems;

[HarmonyPatch(typeof(LifeSuppSystemType), nameof(LifeSuppSystemType.Detoriorate))]
public static class DeteriorateOxygenPatch
{
    public static void Prefix(LifeSuppSystemType __instance)
    {
        if (SabotagePatch.CurrentSabotage?.SabotageType() is SabotageType.Oxygen)
            SabotagePatch.SabotageCountdown = __instance.Countdown;

        if (!__instance.IsActive) return;

        switch (ShipStatus.Instance.Type)
        {
            case ShipStatus.MapType.Ship when GeneralOptions.SabotageOptions.CustomSkeldOxygenCountdown:
                if (__instance.Countdown > GeneralOptions.SabotageOptions.SkeldOxygenCountdown)
                    __instance.Countdown = GeneralOptions.SabotageOptions.SkeldOxygenCountdown;
                break;
            case ShipStatus.MapType.Hq when GeneralOptions.SabotageOptions.CustomMiraOxygenCountdown:
                if (__instance.Countdown > GeneralOptions.SabotageOptions.MiraOxygenCountdown)
                    __instance.Countdown = GeneralOptions.SabotageOptions.MiraOxygenCountdown;
                break;
        }
    }
}
using System;
using HarmonyLib;
using TOHTOR.API.Vanilla.Sabotages;
using TOHTOR.Options;

namespace TOHTOR.Patches.Systems;
//参考
//https://github.com/Koke1024/Town-Of-Moss/blob/main/TownOfMoss/Patches/MeltDownBoost.cs

[HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.Detoriorate))]
public static class ReactorSystemTypePatch
{
    public static void Prefix(ReactorSystemType __instance)
    {
        if (SabotagePatch.CurrentSabotage?.SabotageType() is SabotageType.Reactor)
            SabotagePatch.SabotageCountdown = __instance.Countdown;

        if (!__instance.IsActive) return;

        switch (ShipStatus.Instance.Type)
        {
            case ShipStatus.MapType.Ship when GeneralOptions.SabotageOptions.CustomSkeldReactorCountdown:
                if (__instance.Countdown > GeneralOptions.SabotageOptions.SkeldReactorCountdown)
                    __instance.Countdown = GeneralOptions.SabotageOptions.SkeldReactorCountdown;
                break;
            case ShipStatus.MapType.Hq when GeneralOptions.SabotageOptions.CustomMiraReactorCountdown:
                if (__instance.Countdown > GeneralOptions.SabotageOptions.MiraReactorCountdown)
                    __instance.Countdown = GeneralOptions.SabotageOptions.MiraReactorCountdown;
                break;
            case ShipStatus.MapType.Pb when GeneralOptions.SabotageOptions.CustomPolusReactorCountdown:
                if (__instance.Countdown > GeneralOptions.SabotageOptions.PolusReactorCountdown)
                    __instance.Countdown = GeneralOptions.SabotageOptions.PolusReactorCountdown;
                break;
            default:
                break;
        }
    }
}

[HarmonyPatch(typeof(LifeSuppSystemType), nameof(LifeSuppSystemType.Detoriorate))]
public static class LifeSupportSystemPatch
{
    public static void Prefix(LifeSuppSystemType __instance)
    {
        if (SabotagePatch.CurrentSabotage?.SabotageType() is SabotageType.Oxygen)
            SabotagePatch.SabotageCountdown = __instance.Countdown;
    }
}

[HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.Detoriorate))]
public static class HeliSabotageSystemPatch
{
    public static void Prefix(HeliSabotageSystem __instance)
    {
        if (!__instance.IsActive) return;

        if (AirshipStatus.Instance == null) return;

        if (!GeneralOptions.SabotageOptions.CustomAirshipReactorCountdown) return;

        if (__instance.Countdown > GeneralOptions.SabotageOptions.AirshipReactorCountdown)
            __instance.Countdown = GeneralOptions.SabotageOptions.AirshipReactorCountdown;
    }
}
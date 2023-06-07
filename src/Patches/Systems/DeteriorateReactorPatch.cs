using HarmonyLib;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Options;

namespace Lotus.Patches.Systems;

[HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.Detoriorate))]
public static class DeteriorateReactorPatch
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
        }
    }
}
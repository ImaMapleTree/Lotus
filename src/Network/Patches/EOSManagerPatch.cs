using HarmonyLib;
using Lotus.Network.Komasan.RestClient;

namespace Lotus.Network.Patches;

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsAllowedOnline))]
public class EOSManagerPatch
{
    public static void Postfix(EOSManager __instance, bool canOnline)
    {
        return;
        Komajiro komajiro = Komajiro.Instance;
        if (!canOnline) return;
        komajiro.Initialize(__instance.ProductUserId, __instance.platformAuthToken);
    }
}
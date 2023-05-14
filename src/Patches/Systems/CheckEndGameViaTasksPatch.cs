using HarmonyLib;

namespace Lotus.Patches.Systems;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckEndGameViaTasks))]
public class CheckEndGameViaTasksPatch
{
    public static bool Prefix(GameManager __instance)
    {
        return false;
    }
}
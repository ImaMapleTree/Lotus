using HarmonyLib;
using TOHTOR.Managers;
using VentLib.Logging;

namespace TOHTOR.Patches.Systems;


[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetHighlighted))]
class SetHighlightedPatch
{
    public static bool Prefix(PlayerVoteArea __instance, bool value)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        if (!__instance.HighlightedFX) return false;
        __instance.HighlightedFX.enabled = value;
        return false;
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
class MeetingHudOnDestroyPatch
{
    public static void Postfix()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        AntiBlackout.SetIsDead();
        VentLogger.Debug("------------End of Meeting------------", "Phase");
        /*if (AmongUsClient.Instance.AmHost)
        {
            Game.GetAllPlayers().Do(pc => RandomSpawn.CustomNetworkTransformPatch.NumOfTP[pc.PlayerId] = 0);
        }*/
    }
}
using HarmonyLib;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Vanilla;
using TOHTOR.API.Vanilla.Meetings;
using TOHTOR.Extensions;
using TOHTOR.Options;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Patches.Meetings;


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
        VentLogger.Debug("------------End of Meeting------------", "Phase");

        MeetingDelegate.Instance.BlackscreenResolver.BeginProcess();
        Async.Schedule(PostMeetingSetups, NetUtils.DeriveDelay(0.5f));
    }


    private static void PostMeetingSetups()
    {
        bool noVenting = GeneralOptions.GameplayOptions.ForceNoVenting;
        bool randomSpawn = GeneralOptions.MayhemOptions.RandomSpawn;

        Game.GetAllPlayers().ForEach(p =>
        {
            if (randomSpawn) Game.RandomSpawn.Spawn(p);
            if (noVenting && !p.GetCustomRole().BaseCanVent) Async.Schedule(() => VentApi.ForceNoVenting(p), 0.1f);
        });
    }
}
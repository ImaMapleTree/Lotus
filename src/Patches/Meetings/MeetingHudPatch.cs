using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla;
using Lotus.API.Vanilla.Meetings;
using Lotus.Options;
using Lotus.Extensions;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Patches.Meetings;


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
        bool randomSpawn = GeneralOptions.MayhemOptions.RandomSpawn;

        Game.GetAllPlayers().ForEach(p =>
        {
            if (randomSpawn) Game.RandomSpawn.Spawn(p);
        });
    }
}
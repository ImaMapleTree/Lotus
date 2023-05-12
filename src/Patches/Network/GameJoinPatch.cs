using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using TOHTOR.Addons;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.GUI.Patches;
using VentLib.Logging;
using VentLib.Utilities;

namespace TOHTOR.Patches.Network;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
class GameJoinPatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        Async.Schedule(FriendsListButtonPatch.FixFriendListPosition, 0.1f);
        
        VentLogger.High($"Joining Lobby (GameID={__instance.GameId})", "GameJoin");
        SoundManager.Instance.ChangeMusicVolume(DataManager.Settings.Audio.MusicVolume);

        Game.Cleanup();
        GameStates.InGame = false;
        Async.Schedule(() => AddonManager.VerifyClientAddons(AddonManager.Addons.Select(AddonInfo.From).ToList()), NetUtils.DeriveDelay(0.5f));
    }
}
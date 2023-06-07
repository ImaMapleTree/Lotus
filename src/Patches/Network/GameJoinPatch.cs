using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using Lotus.Addons;
using Lotus.API;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.GUI.Patches;
using Lotus.Managers;
using VentLib.Logging;
using VentLib.Utilities;

namespace Lotus.Patches.Network;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
class GameJoinPatch
{
    private static int _lastGameId;

    public static void Postfix(AmongUsClient __instance)
    {
        Async.Schedule(FriendsListButtonPatch.FixFriendListPosition, 0.1f);

        VentLogger.High($"Joining Lobby (GameID={__instance.GameId})", "GameJoin");
        SoundManager.Instance.ChangeMusicVolume(DataManager.Settings.Audio.MusicVolume);

        GameStates.InGame = false;

        Hooks.NetworkHooks.GameJoinHook.Propagate(new GameJoinHookEvent(_lastGameId != __instance.GameId));
        _lastGameId = __instance.GameId;
        Async.WaitUntil(() => PlayerControl.LocalPlayer, p => p != null, p => PluginDataManager.TitleManager.ApplyTitleWithChatFix(p), 0.1f, 20);
        Async.Schedule(() => AddonManager.VerifyClientAddons(AddonManager.Addons.Select(AddonInfo.From).ToList()), NetUtils.DeriveDelay(0.5f));

        Async.Schedule(PlayerJoinPatch.CheckAutostart, 1f);
    }
}
using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using Lotus.Addons;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Managers;
using LotusTrigger.Options;
using LotusTrigger.Options.General;
using VentLib.Logging;
using VentLib.Utilities;

namespace Lotus.Patches.Network;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
class GameJoinPatch
{
    private static int _lastGameId;

    public static void Postfix(AmongUsClient __instance)
    {
        VentLogger.High($"Joining Lobby (GameID={__instance.GameId})", "GameJoin");
        SoundManager.Instance.ChangeMusicVolume(DataManager.Settings.Audio.MusicVolume);

        GameJoinHookEvent gameJoinHookEvent = new(_lastGameId != __instance.GameId);
        Hooks.NetworkHooks.GameJoinHook.Propagate(gameJoinHookEvent);
        _lastGameId = __instance.GameId;

        Async.WaitUntil(() => PlayerControl.LocalPlayer, p => p != null, p =>
        {
            gameJoinHookEvent.Loaded = true;
            PluginDataManager.TitleManager.ApplyTitleWithChatFix(p);
        }, 0.1f, 20);

        Async.Schedule(() => AddonManager.VerifyClientAddons(AddonManager.Addons.Select(AddonInfo.From).ToList()), NetUtils.DeriveDelay(0.5f));

        if (GeneralOptions.AdminOptions.AutoStartMaxTime != -1)
        {
            GeneralOptions.AdminOptions.AutoCooldown.SetDuration(GeneralOptions.AdminOptions.AutoStartMaxTime);
            GeneralOptions.AdminOptions.AutoCooldown.Start();
        }

        Async.Schedule(PlayerJoinPatch.CheckAutostart, 1f);
    }
}
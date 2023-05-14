using HarmonyLib;
using InnerNet;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Gamemodes;
using Lotus.Managers;
using Lotus.Options;
using Lotus.Utilities;
using VentLib.Logging;
using VentLib.Utilities;
using static Platforms;


namespace Lotus.Patches.Network;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
internal class PlayerJoinPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        VentLogger.Trace($"{client.PlayerName} (ClientID={client.Id}) (Platform={client.PlatformData.PlatformName}) joined the game.", "Session");
        if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost)
        {
            AmongUsClient.Instance.KickPlayer(client.Id, true);
            VentLogger.Old($"ブロック済みのプレイヤー{client?.PlayerName}({client.FriendCode})をBANしました。", "BAN");
        }
        
        Async.WaitUntil(() => client.Character, c => c != null, c => EnforceAdminSettings(client, c), maxRetries: 50);
    }

    private static void EnforceAdminSettings(ClientData client, PlayerControl player)
    {
        bool kickPlayer = false;
        kickPlayer = kickPlayer || GeneralOptions.AdminOptions.KickPlayersWithoutFriendcodes && client.FriendCode == "";
        kickPlayer = kickPlayer || client.PlatformData.Platform is Android or IPhone && GeneralOptions.AdminOptions.KickMobilePlayers;

        if (kickPlayer)
        {
            AmongUsClient.Instance.KickPlayer(client.Id, false);
            return;
        }

        BanManager.CheckBanPlayer(client);
        BanManager.CheckDenyNamePlayer(client);
        
        Hooks.PlayerHooks.PlayerJoinHook.Propagate(new PlayerHookEvent(player));
        player.name = client.PlayerName;
        PluginDataManager.LastKnownAs.SetName(client.FriendCode, client.PlayerName);
        Game.CurrentGamemode.Trigger(GameAction.GameJoin, client);

        if (!GeneralOptions.AdminOptions.AutoStartEnabled || GameStartManager.Instance.LastPlayerCount < GeneralOptions.AdminOptions.AutoStart) return;
        GameStartManager.Instance.BeginGame();
        GameStartManager.Instance.countDownTimer = 0.5f;
    }
}
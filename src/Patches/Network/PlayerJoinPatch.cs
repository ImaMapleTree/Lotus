using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using InnerNet;
using TOHTOR.Addons;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Gamemodes;
using TOHTOR.GUI.Patches;
using TOHTOR.Managers;
using TOHTOR.Options;
using TOHTOR.Utilities;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;
using VentLib.Version;
using static Platforms;
using GameStates = TOHTOR.API.GameStates;


namespace TOHTOR.Patches.Network;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
class OnGameJoinedPatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        Async.Schedule(FriendsListButtonPatch.FixFriendListPosition, 0.1f);

        /*while (!OldOptions.IsLoaded) System.Threading.Tasks.Task.Delay(1);*/
        VentLogger.Old($"{__instance.GameId}に参加", "OnGameJoined");
        TOHPlugin.PlayerVersion = new Dictionary<byte, Version>();
        SoundManager.Instance.ChangeMusicVolume(DataManager.Settings.Audio.MusicVolume);
        /*ChatCommands.ChatHistoryDictionary = new();*/

        /*ChatUpdatePatch.DoBlockChat = false;*/
        GameStates.InGame = false;
        Async.Schedule(() => AddonManager.VerifyClientAddons(AddonManager.Addons.Select(AddonInfo.From).ToList()), NetUtils.DeriveDelay(0.5f));
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
class OnPlayerJoinedPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        VentLogger.Old($"{client.PlayerName}(ClientID:{client.Id})が参加", "Session");
        if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost)
        {
            AmongUsClient.Instance.KickPlayer(client.Id, true);
            VentLogger.Old($"ブロック済みのプレイヤー{client?.PlayerName}({client.FriendCode})をBANしました。", "BAN");
        }


        Async.Schedule(() => EnforceAdminSettings(client), 1f);
    }

    private static void EnforceAdminSettings(ClientData client)
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

        Utils.RunUntilSuccess(() => Hooks.PlayerHooks.PlayerJoinHook.Propagate(new PlayerHookEvent(client.Character)), 0.1f, () => client.Character != null);
        Game.CurrentGamemode.Trigger(GameAction.GameJoin, client);

        if (!GeneralOptions.AdminOptions.AutoStartEnabled || GameStartManager.Instance.LastPlayerCount < GeneralOptions.AdminOptions.AutoStart) return;
        GameStartManager.Instance.BeginGame();
        GameStartManager.Instance.countDownTimer = 0.5f;
    }
}
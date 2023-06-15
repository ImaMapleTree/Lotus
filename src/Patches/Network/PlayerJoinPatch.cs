using System;
using HarmonyLib;
using InnerNet;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Chat;
using Lotus.Gamemodes;
using Lotus.Logging;
using Lotus.Managers;
using Lotus.Utilities;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;
using static Lotus.Options.GeneralOptions;
using static Platforms;


namespace Lotus.Patches.Network;

[LoadStatic]
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
internal class PlayerJoinPatch
{
    private static FixedUpdateLock _autostartLock = new(10f);
    static PlayerJoinPatch()
    {
        PluginDataManager.TemplateManager.RegisterTag("autostart", "Template triggered when the autostart timer begins.");
    }

    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        VentLogger.Trace($"{client.PlayerName} (ClientID={client.Id}) (Platform={client.PlatformData.PlatformName}) joined the game.", "Session");
        if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost)
        {
            AmongUsClient.Instance.KickPlayer(client.Id, true);
            VentLogger.Old($"ブロック済みのプレイヤー{client?.PlayerName}({client.FriendCode})をBANしました。", "BAN");
        }

        Hooks.NetworkHooks.ClientConnectHook.Propagate(new ClientConnectHookEvent(client));
        Async.WaitUntil(() => client.Character, c => c != null, c => EnforceAdminSettings(client, c), maxRetries: 50);
    }

    private static void EnforceAdminSettings(ClientData client, PlayerControl player)
    {
        player.name = client.PlayerName;
        bool kickPlayer = false;
        kickPlayer = kickPlayer || AdminOptions.KickPlayersWithoutFriendcodes && client.FriendCode == "";
        kickPlayer = kickPlayer || client.PlatformData.Platform is Android or IPhone && AdminOptions.KickMobilePlayers;

        if (kickPlayer)
        {
            AmongUsClient.Instance.KickPlayer(client.Id, false);
            return;
        }

        PluginDataManager.BanManager.CheckBanPlayer(client);

        Hooks.PlayerHooks.PlayerJoinHook.Propagate(new PlayerHookEvent(player));
        Game.CurrentGamemode.Trigger(GameAction.GameJoin, client);
        CheckAutostart();
    }

    public static void CheckAutostart()
    {
        if (!AdminOptions.AutoStartEnabled) return;
        if (AdminOptions.AutoStartPlayerThreshold == -1 || PlayerControl.AllPlayerControls.Count < AdminOptions.AutoStartPlayerThreshold)
        {
            if (AdminOptions.AutoStartMaxTime == -1)
            {
                GameStartManager.Instance.ResetStartState();
                return;
            }
            DevLogger.Log(AdminOptions.AutoCooldown.TimeRemaining());
            GameStartManager.Instance.BeginGame();
            float timeRemaining = AdminOptions.AutoCooldown.TimeRemaining();
            GameStartManager.Instance.countDownTimer = timeRemaining;
        }
        else
        {
            if (_autostartLock.AcquireLock()) PluginDataManager.TemplateManager.ShowAll("autostart", PlayerControl.LocalPlayer);
            GameStartManager.Instance.BeginGame();
            GameStartManager.Instance.countDownTimer = AdminOptions.AutoStartGameCountdown;
        }
    }

    [QuickPostfix(typeof(GameStartManager), nameof(GameStartManager.Update))]
    private static void HookStartManager(GameStartManager __instance)
    {
        if (AdminOptions.AutoStartMaxTime == -1) return;
        if (Math.Abs(__instance.countDownTimer - 10f) < 0.5f && _autostartLock.AcquireLock())
            PluginDataManager.TemplateManager.ShowAll("autostart", PlayerControl.LocalPlayer);
    }
}
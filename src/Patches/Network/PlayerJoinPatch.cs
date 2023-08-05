using System;
using HarmonyLib;
using InnerNet;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Logging;
using Lotus.Managers;
using Lotus.Utilities;
using LotusTrigger.Options;
using LotusTrigger.Options.General;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Harmony.Attributes;
using static Platforms;


namespace Lotus.Patches.Network;

[LoadStatic]
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
public class PlayerJoinPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(PlayerJoinPatch));
    private static FixedUpdateLock _autostartLock = new(10f);

    static PlayerJoinPatch()
    {
        PluginDataManager.TemplateManager.RegisterTag("autostart", "Template triggered when the autostart timer begins.");
    }

    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        log.Trace($"{client.PlayerName} (ClientID={client.Id}) (Platform={client.PlatformData.PlatformName}) joined the game.", "Session");
        if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost)
        {
            AmongUsClient.Instance.KickPlayer(client.Id, true);
            log.Info($"ブロック済みのプレイヤー{client?.PlayerName}({client.FriendCode})をBANしました。", "BAN");
        }

        Hooks.NetworkHooks.ClientConnectHook.Propagate(new ClientConnectHookEvent(client));
        Async.WaitUntil(() => client.Character, c => c != null, c => EnforceAdminSettings(client, c), maxRetries: 50);
    }

    private static void EnforceAdminSettings(ClientData client, PlayerControl player)
    {
        player.name = client.PlayerName;
        bool kickPlayer = false;
        kickPlayer = kickPlayer || GeneralOptions.AdminOptions.KickPlayersWithoutFriendcodes && client.FriendCode == "";
        kickPlayer = kickPlayer || client.PlatformData.Platform is Android or IPhone && GeneralOptions.AdminOptions.KickMobilePlayers;

        if (kickPlayer)
        {
            AmongUsClient.Instance.KickPlayer(client.Id, false);
            return;
        }

        PluginDataManager.BanManager.CheckBanPlayer(client);

        Hooks.PlayerHooks.PlayerJoinHook.Propagate(new PlayerHookEvent(player));
        CheckAutostart();
    }

    public static void CheckAutostart()
    {
        if (!GeneralOptions.AdminOptions.AutoStartEnabled) return;
        if (GeneralOptions.AdminOptions.AutoStartPlayerThreshold == -1 || PlayerControl.AllPlayerControls.Count < GeneralOptions.AdminOptions.AutoStartPlayerThreshold)
        {
            if (GeneralOptions.AdminOptions.AutoStartMaxTime == -1)
            {
                GameStartManager.Instance.ResetStartState();
                return;
            }
            DevLogger.Log(GeneralOptions.AdminOptions.AutoCooldown.TimeRemaining());
            GameStartManager.Instance.BeginGame();
            float timeRemaining = GeneralOptions.AdminOptions.AutoCooldown.TimeRemaining();
            GameStartManager.Instance.countDownTimer = timeRemaining;
        }
        else
        {
            if (_autostartLock.AcquireLock()) PluginDataManager.TemplateManager.ShowAll("autostart", PlayerControl.LocalPlayer);
            GameStartManager.Instance.BeginGame();
            GameStartManager.Instance.countDownTimer = GeneralOptions.AdminOptions.AutoStartGameCountdown;
        }
    }

    [QuickPostfix(typeof(GameStartManager), nameof(GameStartManager.Update))]
    private static void HookStartManager(GameStartManager __instance)
    {
        if (GeneralOptions.AdminOptions.AutoStartMaxTime == -1) return;
        if (Math.Abs(__instance.countDownTimer - 10f) < 0.5f && _autostartLock.AcquireLock())
            PluginDataManager.TemplateManager.ShowAll("autostart", PlayerControl.LocalPlayer);
    }
}
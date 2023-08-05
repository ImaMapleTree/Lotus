using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Vanilla.Meetings;
using Lotus.Chat;
using Lotus.RPC;
using Lotus.Utilities;
using Lotus.Victory;
using Lotus.Extensions;
using Lotus.Logging;
using UnityEngine;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers;

internal class BlackscreenResolver
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(BlackscreenResolver));

    private byte resetCameraPlayer = byte.MaxValue;
    private Vector2 resetCameraPosition;
    private MeetingDelegate meetingDelegate;
    private Dictionary<byte, (bool isDead, bool isDisconnected)> playerStates = null!;
    private HashSet<byte> unpatchable;

    public bool Patching;

    private BlackscreenResolver(byte blackscreened)
    {
        unpatchable = new HashSet<byte> { blackscreened };
    }

    internal BlackscreenResolver(MeetingDelegate meetingDelegate)
    {
        this.meetingDelegate = meetingDelegate;
        resetCameraPlayer = Players.GetPlayers(PlayerFilter.Dead).FirstOrOptional().Map(p => p.PlayerId).OrElse(byte.MaxValue);
        Hooks.PlayerHooks.PlayerMessageHook.Bind(nameof(BlackscreenResolver), _ => BlockLavaChat(), true);
        Hooks.GameStateHooks.GameEndHook.Bind(nameof(BlackscreenResolver), _ => Patching = false, true);
    }

    public static bool PerformForcedReset(PlayerControl player)
    {
        BlackscreenResolver blackscreenResolver = new(player.PlayerId);
        blackscreenResolver.TryGetDeadPlayer(out PlayerControl? deadPlayer);
        if (deadPlayer == null) return false;
        blackscreenResolver.StoreAndTeleport(deadPlayer);
        Async.Schedule(() => blackscreenResolver.PerformCameraReset(deadPlayer), 0.1f);
        return true;
    }

    internal void BeginProcess()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        unpatchable = StoreAndSendFallbackData();
        if (unpatchable.Count == 0) return;
        TryGetDeadPlayer(out PlayerControl? deadPlayer);
        if (deadPlayer != null) Async.Schedule(() => StoreAndTeleport(deadPlayer), 1.5f);
    }

    internal void ClearBlackscreen(Action callback)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        float deferredTime = 0; // Used if needing to teleport the player
        Async.Schedule(() => ProcessPatched(callback), NetUtils.DeriveDelay(0.7f));
        if (unpatchable.Count == 0) return;

        bool updated = !TryGetDeadPlayer(out PlayerControl? deadPlayer);
        if (deadPlayer == null) deadPlayer = EmergencyKillHost();

        if (updated)
        {
            StoreAndTeleport(deadPlayer);
            deferredTime = 0.1f;
        }

        log.Debug($"Resetting player cameras using \"{deadPlayer.name}\" as camera player", "BlackscreenResolver");
        Async.Schedule(() => PerformCameraReset(deadPlayer), deferredTime);
    }

    private void PerformCameraReset(PlayerControl deadPlayer)
    {
        log.Debug("Performing Camera Reset", "BlackscreenResolver");
        if (deadPlayer == null) deadPlayer = EmergencyKillHost();

        unpatchable.Filter(b => Utils.PlayerById(b)).ForEach(p =>
        {
            Vector2 originalPosition = p.GetTruePosition();
            p.ResetPlayerCam(0.1f, deadPlayer);
            Async.Schedule(() => Utils.Teleport(p.NetTransform, originalPosition), 1f);
        });

        Async.Schedule(() => Utils.Teleport(deadPlayer.NetTransform, resetCameraPosition), 1f);
    }

    private HashSet<byte> StoreAndSendFallbackData()
    {
        Patching = true;
        byte exiledPlayer = meetingDelegate.ExiledPlayer?.PlayerId ?? byte.MaxValue;
        GameData.PlayerInfo[] playerInfos = GameData.Instance.AllPlayers.ToArray().Where(p => p != null).ToArray();
        playerInfos.FirstOrOptional(p => p.PlayerId == exiledPlayer).IfPresent(info => info.IsDead = true);

        playerStates = playerInfos.ToDict(i => i.PlayerId, i => (i.IsDead, i.Disconnected));
        HashSet<byte> unpatchablePlayers = SendPatchedData(meetingDelegate.ExiledPlayer?.PlayerId ?? byte.MaxValue);
        log.Debug($"Unpatchable Players: {unpatchablePlayers.Filter(p => Utils.PlayerById(p)).Select(p => p.name).Fuse()}", "BlackscreenResolver");
        return unpatchablePlayers;
    }

    private void ProcessPatched(Action callback)
    {
        GameData.Instance.AllPlayers.ToArray().Where(i => i != null).ForEach(info =>
        {
            if (!playerStates.TryGetValue(info.PlayerId, out var val)) return;
            info.IsDead = val.isDead;
            info.Disconnected = val.isDisconnected;
            if (info.Object != null) info.PlayerName = info.Object.name;
        });
        Patching = false;
        GeneralRPC.SendGameData();
        CheckEndGamePatch.Deferred = false;
        callback();
    }

    private bool TryGetDeadPlayer(out PlayerControl? deadPlayer)
    {
        bool ReturnPlayer(PlayerControl player)
        {
            resetCameraPlayer = player.PlayerId;
            return false;
        }

        // First we check the current stored player
        deadPlayer = Utils.GetPlayerById(resetCameraPlayer);
        if (deadPlayer != null) return true;

        // Second we check all the currently dead players
        deadPlayer = Players.GetPlayers(PlayerFilter.Dead).FirstOrDefault();
        if (deadPlayer != null) return ReturnPlayer(deadPlayer);

        // Lastly we check the exiled player
        if (meetingDelegate.ExiledPlayer != null) deadPlayer = meetingDelegate.ExiledPlayer.Object;
        return deadPlayer != null && ReturnPlayer(deadPlayer);
    }

    private void StoreAndTeleport(PlayerControl? cameraPlayer)
    {
        // Quadruple check the player still exists
        if (cameraPlayer == null) TryGetDeadPlayer(out cameraPlayer);
        if (cameraPlayer == null)
        {
            StoreAndSendFallbackData();
            return;
        }
        log.Trace($"Teleporting Camera \"{cameraPlayer.name}\"");
        resetCameraPosition = cameraPlayer.GetTruePosition();
        Utils.Teleport(cameraPlayer.NetTransform, new Vector2(100f, 100f));
    }

    private static HashSet<byte> SendPatchedData(byte exiledPlayer)
    {
        DevLogger.GameInfo();
        HostRpc.RpcDebug("Game Data BEFORE Patch");
        HashSet<byte> unpatchable = AntiBlackoutLogic.PatchedData(exiledPlayer);
        HostRpc.RpcDebug("Game Data AFTER Patch");
        return unpatchable;
    }

    private PlayerControl EmergencyKillHost()
    {
        resetCameraPosition = PlayerControl.LocalPlayer.GetTruePosition();
        StaticLogger.SendInGame("Unable to get an eligible dead player for blackscreen patching. " +
                              "Unfortunately there's nothing further we can do at this point other than killing (you) the host." +
                              "The reasons for this are very complicated, but a lot of code went into preventing this from happening, but it's never guarantees this scenario won't occur.", LogLevel.Fatal);
        PlayerControl.LocalPlayer.MurderPlayer(PlayerControl.LocalPlayer);
        Game.MatchData.UnreportableBodies.Add(PlayerControl.LocalPlayer.PlayerId);
        playerStates[0] = (true, false);
        Async.Schedule(() => PlayerControl.LocalPlayer.RpcVaporize(PlayerControl.LocalPlayer), 6f);
        return PlayerControl.LocalPlayer;
    }

    private void BlockLavaChat()
    {
        if (!Patching) return;
        if (Game.State is GameState.InLobby)
        {
            Patching = false;
            return;
        }
        ChatHandler chatHandler = ChatHandler.Of("", "- Lava Chat Fix -");
        foreach (PlayerControl player in Players.GetPlayers())
        {
            (bool isDead, bool isDisconnected) = playerStates[player.PlayerId];
            if (isDead || isDisconnected) continue;
            for (int i = 0; i < 20; i++) chatHandler.Send(player);
        }
    }
}
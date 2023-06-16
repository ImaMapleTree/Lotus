using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Factions.Crew;
using Lotus.Factions.Impostors;
using Lotus.GUI.Name.Interfaces;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Utilities;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.API.Player;

public static class Players
{
    public static IEnumerable<PlayerControl> GetPlayers(PlayerFilter filter = PlayerFilter.None)
    {
        IEnumerable<PlayerControl> players = Game.GetAllPlayers();
        if (filter.HasFlag(PlayerFilter.NonPhantom)) players = players.Where(p => p.GetCustomRole() is not IPhantomRole pr || pr.IsCountedAsPlayer());
        if (filter.HasFlag(PlayerFilter.Alive)) players = players.Where(p => p.IsAlive());
        if (filter.HasFlag(PlayerFilter.Dead)) players = players.Where(p => !p.IsAlive());
        if (filter.HasFlag(PlayerFilter.Impostor)) players = players.Where(p => p.GetCustomRole().Faction.GetType() == typeof(ImpostorFaction));
        if (filter.HasFlag(PlayerFilter.Crewmate)) players = players.Where(p => p.GetCustomRole().Faction is Crewmates);
        if (filter.HasFlag(PlayerFilter.Neutral)) players = players.Where(p => p.GetCustomRole().SpecialType is SpecialType.Neutral);
        if (filter.HasFlag(PlayerFilter.NeutralKilling)) players = players.Where(p => p.GetCustomRole().SpecialType is SpecialType.NeutralKilling);
        return players;
    }

    public static void SendPlayerData(GameData.PlayerInfo playerInfo, int clientId = -1, bool autoSetName = true)
    {
        INameModel? nameModel = playerInfo.Object != null ? playerInfo.Object.NameModel() : null;
        Game.GetAllPlayers().ForEach(p =>
        {
            int playerClientId = p.GetClientId();
            if (clientId != -1 && playerClientId != clientId) return;

            MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
            messageWriter.StartMessage(6);
            messageWriter.Write(AmongUsClient.Instance.GameId);
            messageWriter.WritePacked(playerClientId);
            messageWriter.StartMessage(1);
            messageWriter.WritePacked(GameData.Instance.NetId);

            string name = playerInfo.PlayerName;
            if (autoSetName) playerInfo.PlayerName = nameModel?.RenderFor(p, sendToPlayer: false, force: true) ?? name;

            messageWriter.StartMessage(playerInfo.PlayerId);
            playerInfo.Serialize(messageWriter);
            messageWriter.EndMessage();

            if (autoSetName) playerInfo.PlayerName = name;

            messageWriter.EndMessage();
            messageWriter.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(messageWriter);
            messageWriter.Recycle();
        });
    }

    public static PlayerControl? FindPlayerById(byte playerId) => Utils.GetPlayerById(playerId);
    public static Optional<PlayerControl> PlayerById(byte playerId) => Utils.PlayerById(playerId);

    public static void SyncAll() => GetPlayers().ForEach(p => p.SyncAll());
}

[Flags]
public enum PlayerFilter
{
    None = 1,
    NonPhantom = 2,
    Alive = 4,
    Dead = 8,
    Impostor = 16,
    Crewmate = 32,
    Neutral = 64,
    NeutralKilling = 128
}
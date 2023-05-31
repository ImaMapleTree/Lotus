using System;
using System.Collections.Generic;
using Hazel;
using Lotus.API.Odyssey;
using Lotus.GUI.Name.Interfaces;
using Lotus.Utilities;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.API.Player;

public static class Players
{
    public static IEnumerable<PlayerControl> GetAllPlayers(PlayerFilter filter = PlayerFilter.None)
    {
        return filter switch
        {
            PlayerFilter.None => Game.GetAllPlayers(),
            PlayerFilter.Alive => Game.GetAlivePlayers(),
            PlayerFilter.Dead => Game.GetDeadPlayers(),
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };
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
}

public enum PlayerFilter
{
    None,
    Alive,
    Dead
}
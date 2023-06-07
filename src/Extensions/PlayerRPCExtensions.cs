using Lotus.API.Player;

namespace Lotus.Extensions;

public static class PlayerRPCExtensions
{
    public static void SetChatName(this PlayerControl player, string name)
    {
        if (player == null) return;
        player.Data.PlayerName = name;
        Players.SendPlayerData(player.Data, autoSetName: false);
    }
}
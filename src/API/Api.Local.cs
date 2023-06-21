using System;
using Lotus.API.Odyssey;
using VentLib.Logging;

namespace Lotus.API;

public partial class Api
{
    public class Local
    {
        public static void SetName(PlayerControl player, string name, bool send = false, GameState state = GameState.None)
        {

            if (player == null) return;
            if (state is GameState.None) state = Game.State;

            if (send)
            {
                player.SetName(name);
                return;
            }

            GameData.PlayerInfo playerData = player.Data;

            if (playerData != null && state is GameState.InMeeting)
            {
                GameData.PlayerOutfit defaultOutfit = playerData.DefaultOutfit;
                defaultOutfit.PlayerName = name;
                AmongUsClient.Instance.GetClientFromCharacter(playerData.Object)?.UpdatePlayerName(name);
            }

            try
            {
                player.cosmetics.nameText.text = name;
                player.cosmetics.SetNameMask(true);
            }
            catch (Exception exception)
            {
                VentLogger.Exception(exception);
            }
        }
    }
}
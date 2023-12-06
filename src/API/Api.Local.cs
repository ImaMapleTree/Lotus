using System;
using Lotus.API.Odyssey;

namespace Lotus.API;

public partial class Api
{
    public class Local
    {
        private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(Local));

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

            if (playerData != null)
            {
                GameData.PlayerOutfit defaultOutfit = playerData.DefaultOutfit;
                string outfitName = state is GameState.InMeeting ? name : player.name;
                defaultOutfit.PlayerName = outfitName;
                AmongUsClient.Instance.GetClientFromCharacter(playerData.Object)?.UpdatePlayerName(outfitName);
            }

            try
            {
                player.cosmetics.nameText.text = name;
                player.cosmetics.SetNameMask(true);
            }
            catch (Exception exception)
            {
                log.Exception(exception);
            }
        }
    }
}
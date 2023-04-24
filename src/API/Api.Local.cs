namespace TOHTOR.API;

public partial class Api
{
    public class Local
    {
        public static void SetName(PlayerControl player, string name, bool send = false)
        {
            if (send)
            {
                player.SetName(name);
                return;
            }

            GameData.PlayerInfo playerData = player.Data;

            if (playerData != null)
            {
                GameData.PlayerOutfit defaultOutfit = playerData.DefaultOutfit;
                defaultOutfit.PlayerName = name;
                AmongUsClient.Instance.GetClientFromCharacter(playerData.Object)?.UpdatePlayerName(name);
            }

            player.gameObject.name = name;
            player.cosmetics.nameText.text = name;
            player.cosmetics.SetNameMask(true);
        }
    }
}
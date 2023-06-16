using System.Collections.Generic;
using HarmonyLib;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Managers;
using Lotus.Options;
using VentLib.Utilities.Extensions;

namespace Lotus.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
class CoStartGamePatch
{
    public static void Prefix(AmongUsClient __instance)
    {
        if (GeneralOptions.MiscellaneousOptions.ColorNameMode) Game.GetAllPlayers().ForEach(player =>
        {
            if (player == null) return;
            string colorName = player.Data.ColorName.Trim('(', ')');
            player.RpcSetName(colorName);
            Api.Local.SetName(player, colorName, true);
        });

        Game.Setup();
    }

    public static void Postfix(AmongUsClient __instance)
    {
        ProjectLotus.ResetCamPlayerList = new List<byte>();
        FallFromLadder.Reset();

        Game.State = GameState.InIntro;
        Game.GetAllPlayers().Do(p => Game.MatchData.Roles.MainRoles[p.PlayerId] = CustomRoleManager.Default);
        Game.CurrentGamemode.Setup();
    }
}
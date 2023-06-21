using System.Collections.Generic;
using System.Linq;
using Lotus.Victory.Conditions;
using Lotus.API.Player;

namespace Lotus.Gamemodes.Colorwars;

public class ColorWarsWinCondition: IWinCondition
{
    public bool IsConditionMet(out List<PlayerControl> winners)
    {
        winners = null;
        // get the colors of all alive players, then get distinct color ids, then if there's 1 id remaining after all that it means a team has won
        List<int> currentColors = Players.GetPlayers(PlayerFilter.Alive).Select(p => p.cosmetics.bodyMatProperties.ColorId).Distinct().ToList();
        if (currentColors.Count != 1) return false;
        int winningColor = currentColors[0];
        winners = Players.GetPlayers().Where(p => p.cosmetics.bodyMatProperties.ColorId == winningColor).ToList();

        return true;
    }

    public WinReason GetWinReason() => new WinReason(ReasonType.GamemodeSpecificWin);
}
using System.Collections.Generic;
using System.Linq;
using Lotus.Factions.Neutrals;
using Lotus.Victory.Conditions;
using Lotus.API.Player;
using Lotus.Extensions;

namespace Lotus.Gamemodes.Standard;

public static class StandardWinConditions
{
    public class SoloRoleWin : IWinCondition
    {
        public bool IsConditionMet(out List<PlayerControl> winners)
        {
            winners = null;
            List<PlayerControl> allPlayers = Players.GetPlayers().ToList();
            if (allPlayers.Count != 1) return false;

            PlayerControl lastPlayer = allPlayers[0];
            winners = new List<PlayerControl> { lastPlayer };
            return lastPlayer.GetCustomRole().Faction is Neutral;
        }

        public WinReason GetWinReason() => new(ReasonType.FactionLastStanding);
    }



    public class LoversWin : IWinCondition
    {
        public bool IsConditionMet(out List<PlayerControl> winners)
        {
            winners = null;
            return false;
            /*if (Players.GetPlayers(PlayerFilter.Alive).Count() > 3) return false;
            List<PlayerControl> lovers = Game.FindAlivePlayersWithRole(CustomRoleManager.Special.LoversReal).ToList();
            if (lovers.Count != 2) return false;
            LoversReal loversRealRole = lovers[0].GetSubrole<LoversReal>()!;
            winners = lovers;
            return loversRealRole.Partner != null && loversRealRole.Partner.PlayerId == lovers[1].PlayerId;*/
        }

        public WinReason GetWinReason() => new(ReasonType.RoleSpecificWin);

        public int Priority() => 100;
    }

}
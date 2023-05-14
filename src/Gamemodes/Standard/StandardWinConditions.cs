using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Factions.Neutrals;
using Lotus.Managers;
using Lotus.Roles.Subroles;
using Lotus.Victory.Conditions;
using Lotus.API;
using Lotus.Extensions;
using Lotus.Roles.Legacy;

namespace Lotus.Gamemodes.Standard;

public static class StandardWinConditions
{
    public class SoloRoleWin : IWinCondition
    {
        public bool IsConditionMet(out List<PlayerControl> winners)
        {
            winners = null;
            List<PlayerControl> allPlayers = Game.GetAllPlayers().ToList();
            if (allPlayers.Count != 1) return false;

            PlayerControl lastPlayer = allPlayers[0];
            return lastPlayer.GetCustomRole().Faction is Solo;
        }

        public WinReason GetWinReason() => WinReason.FactionLastStanding;
    }

    

    public class LoversWin : IWinCondition
    {
        public bool IsConditionMet(out List<PlayerControl> winners)
        {
            winners = null;
            if (Game.GetAlivePlayers().Count() > 3) return false;
            List<PlayerControl> lovers = Game.FindAlivePlayersWithRole(CustomRoleManager.Special.LoversReal).ToList();
            if (lovers.Count != 2) return false;
            LoversReal loversRealRole = lovers[0].GetSubrole<LoversReal>()!;
            winners = lovers;
            return loversRealRole.Partner != null && loversRealRole.Partner.PlayerId == lovers[1].PlayerId;
        }

        public WinReason GetWinReason() => WinReason.RoleSpecificWin;

        public int Priority() => 100;
    }

}
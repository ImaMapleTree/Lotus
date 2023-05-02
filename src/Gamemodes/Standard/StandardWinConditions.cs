using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions.Neutrals;
using TOHTOR.Managers;
using TOHTOR.Roles.Legacy;
using TOHTOR.Roles.Subroles;
using TOHTOR.Victory.Conditions;

namespace TOHTOR.Gamemodes.Standard;

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

    public class SoloKillingWin : IWinCondition
    {
        public bool IsConditionMet(out List<PlayerControl> winners)
        {
            winners = null;
            List<PlayerControl> alivePlayers = Game.GetAlivePlayers().ToList();
            if (alivePlayers.Count > 2 || GameStates.CountAliveImpostors() > 0) return false;

            List<PlayerControl> soloKilling = alivePlayers.Where(p => p.GetCustomRole().Faction is Solo && p.GetVanillaRole().IsImpostor()).ToList();
            if (soloKilling.Count != 1) return false;
            winners = new List<PlayerControl> { soloKilling[0] };
            return true;
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
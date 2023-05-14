using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Roles;
using Lotus.API;
using Lotus.Extensions;

namespace Lotus.Victory.Conditions;

public class VanillaImpostorWin: IFactionWinCondition
{
    private static readonly List<IFaction> ImpostorFaction = new() { FactionInstances.Impostors };
    public bool IsConditionMet(out List<IFaction> factions)
    {
        factions = ImpostorFaction;

        if (Game.State is not GameState.Roaming) return false;

        int aliveImpostors = 0;
        int aliveKillers = 0;
        int aliveOthers = 0;

        foreach (CustomRole role in Game.GetAlivePlayers().Select(p => p.GetCustomRole()))
        {
            if (role.Faction.Relationship(FactionInstances.Impostors) is Relation.FullAllies or Relation.SharedWinners) aliveImpostors++;
            else
            {
                aliveOthers++;
                if (role.Faction.Relationship(FactionInstances.Crewmates) is Relation.FullAllies) continue;
                if (role.MyPlayer.GetVanillaRole().IsImpostor()) aliveKillers++;
            }
            
        }

        return aliveImpostors > 0 && aliveImpostors >= aliveOthers && aliveImpostors > aliveKillers;
    }

    public WinReason GetWinReason() => WinReason.FactionLastStanding;
}
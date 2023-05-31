using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Roles;
using Lotus.Extensions;

namespace Lotus.Victory.Conditions;

public class VanillaImpostorWin: IFactionWinCondition
{
    private static readonly List<IFaction> ImpostorFaction = new() { FactionInstances.Impostors };

    public List<IFaction> Factions() => ImpostorFaction;

    public bool IsConditionMet(out List<IFaction> factions)
    {
        factions = ImpostorFaction;

        if (Game.State is not (GameState.Roaming or GameState.InMeeting)) return false;

        int aliveImpostors = 0;
        int aliveKillers = 0;
        int aliveOthers = 0;

        foreach (CustomRole role in Game.GetAlivePlayers().Select(p => p.GetCustomRole()))
        {
            if (role.Faction.Relationship(FactionInstances.Impostors) is Relation.FullAllies or Relation.SharedWinners) aliveImpostors++;
            else
            {
                aliveOthers++;
                if (role.RoleFlags.HasFlag(RoleFlag.CannotWinAlone)) continue;
                if (role.Faction.Relationship(FactionInstances.Crewmates) is Relation.FullAllies) continue;
                if (role.MyPlayer.GetVanillaRole().IsImpostor()) aliveKillers++;
            }
        }

        return aliveImpostors > 0 && aliveImpostors >= aliveOthers && aliveKillers == 0;
    }

    public WinReason GetWinReason() => WinReason.FactionLastStanding;
}
using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Roles;

namespace TOHTOR.Victory.Conditions;

public class VanillaImpostorWin: IFactionWinCondition
{
    private static readonly List<IFaction> ImpostorFaction = new() { FactionInstances.Impostors };
    public bool IsConditionMet(out List<IFaction> factions)
    {
        factions = ImpostorFaction;

        int aliveImpostors = 0;
        int aliveOthers = 0;

        foreach (CustomRole role in Game.GetAlivePlayers().Select(p => p.GetCustomRole()))
            if (role.Faction.Relationship(FactionInstances.Impostors) is Relation.FullAllies or Relation.SharedWinners) aliveImpostors++;
            else aliveOthers++;

        return aliveImpostors >= aliveOthers;
    }

    public WinReason GetWinReason() => WinReason.FactionLastStanding;
}
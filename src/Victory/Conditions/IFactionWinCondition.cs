using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Interfaces;

namespace TOHTOR.Victory.Conditions;

public interface IFactionWinCondition: IWinCondition
{
    bool IWinCondition.IsConditionMet(out List<PlayerControl> winners)
    {
        winners = null;
        if (!IsConditionMet(out List<IFaction> factions)) return false;
        winners = Game.GetAllPlayers()
            .Where(p => factions.Any(f => f.Relationship(p.GetCustomRole().Faction) is Relation.SharedWinners or Relation.FullAllies))
            .ToList();
        return true;
    }

    bool IsConditionMet(out List<IFaction> factions);
}
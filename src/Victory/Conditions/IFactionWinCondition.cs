using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.API;
using Lotus.Extensions;

namespace Lotus.Victory.Conditions;

public interface IFactionWinCondition: IWinCondition
{
    List<IFaction> Factions();

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
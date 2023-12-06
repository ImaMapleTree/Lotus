using System.Collections.Generic;
using System.Linq;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.API.Player;
using Lotus.Extensions;

namespace Lotus.Victory.Conditions;

public interface IFactionWinCondition: IWinCondition
{
    List<IFaction> Factions();

    bool IWinCondition.IsConditionMet(out List<PlayerControl> winners)
    {
        winners = null;
        if (!IsConditionMet(out List<IFaction> factions)) return false;
        winners = Players.GetPlayers()
            .Where(p => factions.Any(f => f.Relationship(p.PrimaryRole().Faction) is Relation.SharedWinners or Relation.FullAllies))
            .ToList();
        return true;
    }

    bool IsConditionMet(out List<IFaction> factions);
}
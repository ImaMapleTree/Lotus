using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Player;

namespace Lotus.Victory.Conditions;

public class FallbackCondition: IWinCondition
{
    private readonly List<PlayerControl> noWinners = new();

    public bool IsConditionMet(out List<PlayerControl> winners)
    {
        winners = noWinners;
        return !Players.GetPlayers(PlayerFilter.Alive).Any();
    }

    public WinReason GetWinReason() => new(ReasonType.NoWinCondition);

    public int Priority() => Int32.MinValue;
}
using System.Collections.Generic;
using Lotus.API.Player;

namespace Lotus.API.Reactive.HookEvents;

public class LosersHookEvent: IHookEvent
{
    public List<FrozenPlayer> Losers;

    public LosersHookEvent(List<FrozenPlayer> losers)
    {
        Losers = losers;
    }
}
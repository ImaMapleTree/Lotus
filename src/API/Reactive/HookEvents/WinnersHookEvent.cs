using System.Collections.Generic;
using Lotus.API.Player;

namespace Lotus.API.Reactive.HookEvents;

public class WinnersHookEvent: IHookEvent
{
    public List<FrozenPlayer> Winners;

    public WinnersHookEvent(List<FrozenPlayer> winners)
    {
        Winners = winners;
    }
}
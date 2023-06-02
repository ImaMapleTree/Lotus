using Lotus.Managers.History.Events;

namespace Lotus.API.Reactive.HookEvents;

public class PlayerDeathHookEvent: PlayerHookEvent
{
    public IDeathEvent CauseOfDeath;

    public PlayerDeathHookEvent(PlayerControl player, IDeathEvent causeOfDeath) : base(player)
    {
        CauseOfDeath = causeOfDeath;
    }
}
using Lotus.Managers.History.Events;

namespace Lotus.API.Reactive.HookEvents;

public class PlayerMurderHookEvent: PlayerDeathHookEvent
{
    public PlayerControl Killer;
    public PlayerControl Victim;

    public PlayerMurderHookEvent(PlayerControl killer, PlayerControl victim, IDeathEvent causeOfDeath) : base(victim, causeOfDeath)
    {
        Killer = killer;
        Victim = victim;
    }
}
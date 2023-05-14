namespace Lotus.API.Reactive.HookEvents;

public class PlayerMurderHookEvent: IHookEvent
{
    public PlayerControl Killer;
    public PlayerControl Victim;

    public PlayerMurderHookEvent(PlayerControl killer, PlayerControl victim)
    {
        Killer = killer;
        Victim = victim;
    }
}
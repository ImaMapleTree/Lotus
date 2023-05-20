namespace Lotus.API.Reactive.HookEvents;

public class PlayerDeathHookEvent: PlayerHookEvent
{
    public string CauseOfDeath;
    
    public PlayerDeathHookEvent(PlayerControl player, string causeOfDeath) : base(player)
    {
        CauseOfDeath = causeOfDeath;
    }
}
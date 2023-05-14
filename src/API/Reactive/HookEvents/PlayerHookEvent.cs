namespace Lotus.API.Reactive.HookEvents;

public class PlayerHookEvent : IHookEvent
{
    public PlayerControl Player;

    public PlayerHookEvent(PlayerControl player)
    {
        Player = player;
    }
}
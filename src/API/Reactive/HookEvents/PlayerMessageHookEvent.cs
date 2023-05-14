namespace Lotus.API.Reactive.HookEvents;

public class PlayerMessageHookEvent : IHookEvent
{
    public PlayerControl Player;
    public string Message;

    public PlayerMessageHookEvent(PlayerControl player, string message)
    {
        Player = player;
        Message = message;
    }
}
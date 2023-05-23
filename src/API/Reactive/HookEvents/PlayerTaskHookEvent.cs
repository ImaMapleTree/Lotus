namespace Lotus.API.Reactive.HookEvents;

public class PlayerTaskHookEvent: IHookEvent
{
    public PlayerControl Player;
    public NormalPlayerTask? PlayerTask;

    public PlayerTaskHookEvent(PlayerControl player, NormalPlayerTask? playerTask)
    {
        Player = player;
        PlayerTask = playerTask;
    }
}
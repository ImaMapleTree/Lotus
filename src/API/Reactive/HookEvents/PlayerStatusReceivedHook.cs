using Lotus.Statuses;

namespace Lotus.API.Reactive.HookEvents;

public class PlayerStatusReceivedHook: PlayerHookEvent
{
    public PlayerControl? Infector;
    public IStatus Status;

    public PlayerStatusReceivedHook(PlayerControl target, IStatus status, PlayerControl? infector = null) : base(target)
    {
        Infector = infector;
        Status = status;
    }
}
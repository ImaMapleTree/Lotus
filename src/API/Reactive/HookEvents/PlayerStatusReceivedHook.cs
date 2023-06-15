using Lotus.Statuses;

namespace Lotus.API.Reactive.HookEvents;

public class PlayerStatusReceivedHook: PlayerHookEvent
{
    public PlayerControl Infector;
    public IStatus Status;

    public PlayerStatusReceivedHook(PlayerControl target, PlayerControl infector, IStatus status) : base(target)
    {
        Infector = infector;
        Status = status;
    }
}
using VentLib.Version;

namespace Lotus.API.Reactive.HookEvents;

public class ReceiveVersionHookEvent: IHookEvent
{
    public PlayerControl Player;
    public Version Version;

    public ReceiveVersionHookEvent(PlayerControl player, Version version)
    {
        Player = player;
        Version = version;
    }
}
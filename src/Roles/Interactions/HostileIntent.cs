using Lotus.Roles.Interactions.Interfaces;
using Lotus.Extensions;

namespace Lotus.Roles.Interactions;

public class HostileIntent : IHostileIntent
{
    public void Action(PlayerControl actor, PlayerControl target)
    {
    }

    public void Halted(PlayerControl actor, PlayerControl target)
    {
        actor.RpcMark(actor);
    }
}
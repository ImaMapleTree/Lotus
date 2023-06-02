using System;
using Lotus.Managers.History.Events;

namespace Lotus.Roles.Interactions;

// Eventually fatal IE AgiTater bomb
public class FakeFatalIntent : FatalIntent
{
    public FakeFatalIntent(bool ranged = false, Func<IDeathEvent>? causeOfDeath = null) : base(ranged, causeOfDeath)
    {
    }


    public override void Action(PlayerControl actor, PlayerControl target)
    {
    }
}
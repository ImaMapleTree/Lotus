using System;
using TOHTOR.Managers.History.Events;

namespace TOHTOR.Roles.Interactions;

// Eventually fatal IE AgiTater bomb
public class EfIntent : FatalIntent
{
    public EfIntent(bool ranged = false, Func<IDeathEvent>? causeOfDeath = null) : base(ranged, causeOfDeath)
    {
    }


    public override void Action(PlayerControl actor, PlayerControl target)
    {
    }
}
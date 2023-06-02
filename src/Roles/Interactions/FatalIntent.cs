using System;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Extensions;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Interactions;

public class FatalIntent : IFatalIntent
{
    private Func<IDeathEvent>? causeOfDeath;
    private bool ranged;

    public FatalIntent(bool ranged = false, Func<IDeathEvent>? causeOfDeath = null)
    {
        this.ranged = ranged;
        this.causeOfDeath = causeOfDeath;
    }

    public Optional<IDeathEvent> CauseOfDeath() => Optional<IDeathEvent>.Of(causeOfDeath?.Invoke());

    public bool IsRanged() => ranged;

    public virtual void Action(PlayerControl actor, PlayerControl target)
    {
        Optional<IDeathEvent> deathEvent = CauseOfDeath();
        actor.GetCustomRole().SyncOptions();

        Optional<IDeathEvent> currentDeathEvent = Game.MatchData.GameHistory.GetCauseOfDeath(target.PlayerId);
        deathEvent.IfPresent(death => Game.MatchData.GameHistory.SetCauseOfDeath(target.PlayerId, death));
        KillTarget(actor, target);

        ActionHandle ignored = ActionHandle.NoInit();
        if (target.IsAlive()) Game.TriggerForAll(RoleActionType.SuccessfulAngelProtect, ref ignored, target, actor);
        else currentDeathEvent.IfPresent(de => Game.MatchData.GameHistory.SetCauseOfDeath(target.PlayerId, de));
    }

    public void KillTarget(PlayerControl actor, PlayerControl target)
    {
        ProtectedRpc.CheckMurder(!ranged ? actor : target, target);
    }

    public void Halted(PlayerControl actor, PlayerControl target)
    {
        actor.RpcMark(target);
    }
}
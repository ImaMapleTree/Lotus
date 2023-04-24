using System;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Roles.Interactions;

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

        if (!target.GetCustomRole().CanBeKilled())
        {
            actor.RpcGuardAndKill(target);
            return;
        }

        Optional<IDeathEvent> currentDeathEvent = Game.GameHistory.GetCauseOfDeath(target.PlayerId);
        deathEvent.IfPresent(death => Game.GameHistory.SetCauseOfDeath(target.PlayerId, death));
        KillTarget(actor, target);
        ActionHandle ignored = ActionHandle.NoInit();
        if (target.IsAlive()) Game.TriggerForAll(RoleActionType.SuccessfulAngelProtect, ref ignored, target, actor);
        else currentDeathEvent.IfPresent(de => Game.GameHistory.SetCauseOfDeath(target.PlayerId, de));
    }

    public void KillTarget(PlayerControl actor, PlayerControl target)
    {
        ProtectedRpc.CheckMurder(!ranged ? actor : target, target);
    }

    public void Halted(PlayerControl actor, PlayerControl target)
    {
        actor.RpcGuardAndKill(target);
    }
}
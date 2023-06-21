using System;
using System.Collections.Generic;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Patches.Actions;
using Lotus.Roles.Internals.Enums;
using VentLib.Utilities.Extensions;
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

        deathEvent.IfPresent(ev => Game.MatchData.GameHistory.SetCauseOfDeath(target.PlayerId, ev));
        KillTarget(actor, target);

        ActionHandle ignored = ActionHandle.NoInit();
        if (!target.IsAlive()) return;
        DevLogger.Log("Target was still alive :O");
        Game.TriggerForAll(RoleActionType.SuccessfulAngelProtect, ref ignored, target, actor);
        Game.MatchData.GameHistory.ClearCauseOfDeath(target.PlayerId);
    }

    public void KillTarget(PlayerControl actor, PlayerControl target)
    {
        ProtectedRpc.CheckMurder(!ranged ? actor : target, target);
    }

    public void Halted(PlayerControl actor, PlayerControl target)
    {
        actor.RpcMark(target);
    }

    private Dictionary<string, object?>? meta;
    public object? this[string key]
    {
        get => (meta ?? new Dictionary<string, object?>()).GetValueOrDefault(key);
        set => (meta ?? new Dictionary<string, object?>())[key] = value;
    }
}
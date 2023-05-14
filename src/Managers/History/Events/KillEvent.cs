using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.API;
using Lotus.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public class KillEvent : IKillEvent
{
    private PlayerControl killer;
    private Optional<CustomRole> killerRole;

    private PlayerControl victim;
    private Optional<CustomRole> victimRole;

    private bool successful;
    private Timestamp timestamp = new();

    public KillEvent(PlayerControl killer, PlayerControl victim, bool successful = true)
    {
        this.killer = killer;
        killerRole = Optional<CustomRole>.Of(killer.GetCustomRole());
        this.victim = victim;
        victimRole = Optional<CustomRole>.Of(victim.GetCustomRole());
        this.successful = successful;
    }

    public PlayerControl Player() => this.killer;

    public Optional<CustomRole> RelatedRole() => this.killerRole;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => successful;

    public virtual string Message() => $"{Game.GetName(killer)} killed {Game.GetName(victim)}.";

    public PlayerControl Target() => victim;

    public Optional<CustomRole> TargetRole() => victimRole;
}
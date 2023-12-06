using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.Extensions;
using Lotus.Roles2;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public class KillEvent : IKillEvent
{
    private PlayerControl killer;
    private Optional<UnifiedRoleDefinition> killerRole;

    private PlayerControl victim;
    private Optional<UnifiedRoleDefinition> victimRole;

    private bool successful;
    private Timestamp timestamp = new();

    public KillEvent(PlayerControl killer, PlayerControl victim, bool successful = true)
    {
        this.killer = killer;
        killerRole = Optional<UnifiedRoleDefinition>.Of(killer.PrimaryRole());
        this.victim = victim;
        victimRole = Optional<UnifiedRoleDefinition>.Of(victim.PrimaryRole());
        this.successful = successful;
    }

    public PlayerControl Player() => this.killer;

    public Optional<UnifiedRoleDefinition> RelatedRole() => this.killerRole;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => successful;

    public virtual string Message() => $"{Game.GetName(killer)} killed {Game.GetName(victim)}.";

    public PlayerControl Target() => victim;

    public Optional<UnifiedRoleDefinition> TargetRole() => victimRole;
}
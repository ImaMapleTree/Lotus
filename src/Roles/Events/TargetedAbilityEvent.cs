using Lotus.Managers.History;
using Lotus.Managers.History.Events;
using Lotus.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Events;

public abstract class TargetedAbilityEvent : ITargetedEvent, IRoleEvent
{
    private PlayerControl source;
    private Optional<CustomRole> sourceRole;

    private PlayerControl target;
    private Optional<CustomRole> targetRole;

    private Timestamp timestamp = new();
    private bool success;


    public TargetedAbilityEvent(PlayerControl source, PlayerControl target, bool successful = true)
    {
        this.source = source;
        sourceRole = Optional<CustomRole>.Of(source.GetCustomRole());

        this.target = target;
        targetRole = Optional<CustomRole>.Of(target.GetCustomRole());
        success = successful;
    }

    public PlayerControl Player() => source;

    public Optional<CustomRole> RelatedRole() => sourceRole;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => success;

    public abstract string Message();

    public PlayerControl Target() => target;

    public Optional<CustomRole> TargetRole() => targetRole;
}
using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.API;
using Lotus.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public class ProtectEvent : ITargetedEvent, IRoleEvent
{
    private PlayerControl protector;
    private Optional<CustomRole> protectorRole;

    private PlayerControl target;
    private Optional<CustomRole> targetRole;

    private Timestamp timestamp = new();

    public ProtectEvent(PlayerControl protector, PlayerControl target)
    {
        this.protector = protector;
        protectorRole = Optional<CustomRole>.Of(protector.GetCustomRole());
        this.target = target;
        targetRole = Optional<CustomRole>.Of(target.GetCustomRole());
    }

    public PlayerControl Player() => protector;

    public Optional<CustomRole> RelatedRole() => protectorRole;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => true;

    public string Message() => $"{Game.GetName(protector)} began protecting {Game.GetName(target)}";

    public PlayerControl Target() => target;

    public Optional<CustomRole> TargetRole() => targetRole;
}
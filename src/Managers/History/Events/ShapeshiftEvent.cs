using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.API;
using Lotus.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public class ShapeshiftEvent : ITargetedEvent
{
    private PlayerControl source;
    private Optional<CustomRole> sourceRole;

    private PlayerControl target;
    private Optional<CustomRole> targetRole;

    private Timestamp timestamp;

    public ShapeshiftEvent(PlayerControl source, PlayerControl target)
    {
        this.source = source;
        sourceRole = Optional<CustomRole>.Of(source.GetCustomRole());
        this.target = target;
        targetRole = Optional<CustomRole>.Of(target.GetCustomRole());
        this.timestamp = new Timestamp();
    }

    public PlayerControl Player() => source;

    public Optional<CustomRole> RelatedRole() => sourceRole;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => true;

    public string Message() => $"{Game.GetName(source)} shapeshifted into {Game.GetName(target)}";

    public PlayerControl Target() => target;

    public Optional<CustomRole> TargetRole() => targetRole;
}
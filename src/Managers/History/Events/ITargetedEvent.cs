using Lotus.Roles2;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public interface ITargetedEvent : IHistoryEvent
{
    public PlayerControl Target();

    public Optional<UnifiedRoleDefinition> TargetRole();
}
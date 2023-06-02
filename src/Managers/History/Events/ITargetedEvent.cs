using Lotus.Roles;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public interface ITargetedEvent : IHistoryEvent
{
    public PlayerControl Target();

    public Optional<CustomRole> TargetRole();
}
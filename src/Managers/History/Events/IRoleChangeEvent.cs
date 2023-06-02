using Lotus.Roles;

namespace Lotus.Managers.History.Events;

public interface IRoleChangeEvent : IHistoryEvent
{
    public CustomRole OriginalRole();

    public CustomRole NewRole();
}
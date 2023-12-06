using Lotus.API.Player;
using Lotus.Roles;
using Lotus.Roles2;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public interface IRecipientEvent : IHistoryEvent
{
    public Optional<FrozenPlayer> Instigator();

    public Optional<UnifiedRoleDefinition> InstigatorRole();
}
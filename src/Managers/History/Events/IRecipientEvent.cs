using Lotus.Roles;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public interface IRecipientEvent : IHistoryEvent
{
    public Optional<PlayerControl> Instigator();

    public Optional<CustomRole> InstigatorRole();
}
using Lotus.Managers.History.Events;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Interactions.Interfaces;

public interface IFatalIntent : Intent
{
    public Optional<IDeathEvent> CauseOfDeath();

    public bool IsRanged();
}
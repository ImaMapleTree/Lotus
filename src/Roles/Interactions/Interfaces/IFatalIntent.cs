using TOHTOR.Managers.History.Events;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Roles.Interactions.Interfaces;

public interface IFatalIntent : Intent
{
    public Optional<IDeathEvent> CauseOfDeath();

    public bool IsRanged();
}
using Lotus.Managers.History.Events;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Interactions.Interfaces;

/// <summary>
/// This intent is not one of the standard intents, but SHOULD be the backing behind all intents that leave a player dead via their Action() call.
/// </summary>
public interface IKillingIntent: Intent
{
    public Optional<IDeathEvent> CauseOfDeath();
}
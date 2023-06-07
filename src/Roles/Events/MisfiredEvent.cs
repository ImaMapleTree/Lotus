using Lotus.Managers.History.Events;

namespace Lotus.Roles.Events;

public class MisfiredEvent: DeathEvent
{
    public MisfiredEvent(PlayerControl deadPlayer) : base(deadPlayer, deadPlayer)
    {
    }

    public override string SimpleName() => ModConstants.DeathNames.Misfired;

    public override string Message() => $"{Player().name} misfired.";
}
namespace Lotus.Managers.History.Events;

public class CustomDeathEvent: DeathEvent
{
    private string deathType;

    public CustomDeathEvent(PlayerControl deadPlayer, PlayerControl? killer, string deathType) : base(deadPlayer, killer)
    {
        this.deathType = deathType;
    }

    public override string SimpleName() => deathType;
}
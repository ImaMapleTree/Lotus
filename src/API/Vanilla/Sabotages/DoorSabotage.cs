using VentLib.Utilities.Optionals;

namespace TOHTOR.API.Vanilla.Sabotages;

public class DoorSabotage : ISabotage
{
    public SabotageType SabotageType() => Sabotages.SabotageType.Door;
    public bool Fix(PlayerControl? fixer = null)
    {
        throw new System.NotImplementedException();
    }

    public Optional<PlayerControl> Caller()
    {
        throw new System.NotImplementedException();
    }

    public void Sabotage(PlayerControl sabotageCaller)
    {
        throw new System.NotImplementedException();
    }
}
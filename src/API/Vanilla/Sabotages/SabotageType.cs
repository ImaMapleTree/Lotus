using System;

namespace Lotus.API.Vanilla.Sabotages;

[Flags]
public enum SabotageType
{
    Lights = 1,
    Communications = 2,
    Oxygen = 4,
    Reactor = 8,
    Door = 16,
    Helicopter = 32
}


public static class SabotageTypeMethods
{
    public static SystemTypes ToSystemType(this SabotageType sabotageType)
    {
        return sabotageType switch
        {
            SabotageType.Lights => SystemTypes.Electrical,
            SabotageType.Communications => SystemTypes.Comms,
            SabotageType.Oxygen => SystemTypes.LifeSupp,
            SabotageType.Reactor => GameOptionsManager.Instance.CurrentGameOptions.MapId == 2
                ? SystemTypes.Laboratory
                : SystemTypes.Reactor,
            SabotageType.Door => SystemTypes.Doors,
            SabotageType.Helicopter => SystemTypes.Reactor,
            _ => throw new ArgumentOutOfRangeException(nameof(sabotageType), sabotageType, null)
        };
    }
}
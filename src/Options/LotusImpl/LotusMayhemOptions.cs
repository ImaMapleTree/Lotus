using System;

namespace Lotus.Options.LotusImpl;

public class LotusMayhemOptions: LotusOptionModel
{
    public AuMap RandomMaps;
    public bool RandomSpawn;

    public virtual bool UseRandomMap => randomMapOn && RandomMaps != 0;

    protected bool randomMapOn;

}

[Flags]
public enum AuMap
{
    Skeld = 1,
    Mira = 2,
    Polus = 4,
    Airship = 8
}
using UnityEngine;

namespace Lotus.Factions.Interfaces;

public interface IFaction
{
    public string Name();

    public Relation Relationship(IFaction other);

    public bool AlliesSeeRole();

    public Color FactionColor();
}

public interface IFaction<in T> : IFaction where T : IFaction<T>
{
    public Relation Relationship(T sameFaction);
}
using Lotus.Roles;
using UnityEngine;

namespace Lotus.Factions.Interfaces;

public interface IFaction
{
    public string Name();

    public Relation Relationship(IFaction other);

    public Relation Relationship(CustomRole otherRole);

    public bool CanSeeRole(PlayerControl player);

    public Color FactionColor();
}

public interface IFaction<in T> : IFaction where T : IFaction<T>
{
    public Relation Relationship(T sameFaction);
}
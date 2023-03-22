namespace TOHTOR.Factions.Interfaces;

public interface IFaction
{
    public string Name();

    public Relation Relationship(IFaction other);

    public bool AlliesSeeRole();
}

public interface IFaction<in T> : IFaction where T : IFaction<T>
{
    public Relation Relationship(T sameFaction);
}
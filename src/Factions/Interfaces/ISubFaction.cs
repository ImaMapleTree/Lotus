namespace TOHTOR.Factions.Interfaces;

public interface ISubFaction<T> where T: IFaction<T>
{
    public string Name();

    public Relation MainFactionRelationship();

    public Relation Relationship(ISubFaction<T> subFaction);
}
/*namespace TOHTOR.Factions;

public interface IFaction<T> : IFaction where T:IFaction<T>
{
    public T InnerFaction();

    public bool IsAllied(IFaction<T> sameFaction);

    public bool IsAllied(ISubFaction<T> subFaction);

    public ISubFaction<T>[] SubFactions();
}

public interface IFaction
{
    public string Name();

    public IFaction MainFaction();

    public bool IsAllied(IFaction other);

    public bool WinsWith(IFaction other);
}*/
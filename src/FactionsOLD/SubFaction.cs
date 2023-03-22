/*namespace TOHTOR.Factions;

public abstract class SubFaction<T>: ISubFaction<T> where T: IFaction<T>
{
    private T parentFaction;

    public SubFaction(T parentFaction)
    {
        this.parentFaction = parentFaction;
    }

    public string Name() => GetType().Name;

    public IFaction MainFaction() => parentFaction;

    public abstract bool WinsWith(IFaction other);

    public T InnerFaction() => parentFaction;

    public bool IsAllied(IFaction<T> sameFaction) => parentFaction.IsAllied(sameFaction);

    public ISubFaction<T>[] SubFactions() => parentFaction.SubFactions();

    public abstract bool IsAllied(ISubFaction<T> subFaction);
}*/
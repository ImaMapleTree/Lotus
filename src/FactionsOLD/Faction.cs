/*namespace TOHTOR.FactionsOLD;

public abstract class Faction<T> : IFaction<T> where T: Faction<T>
{
    private T instance;
    private ISubFaction<T>[] subFactions;

    public Faction()
    {
        instance = (T)this;
        subFactions = SupplySubFactions();
    }

    public abstract string Name();

    public IFaction MainFaction() => instance;

    protected abstract ISubFaction<T>[] SupplySubFactions();

    public bool IsAllied(IFaction other)
    {
        return other switch
        {
            T faction => IsAllied(faction),
            ISubFaction<T> subFaction => IsAllied(subFaction),
            _ => IsAlliedWith(other)
        };
    }

    public abstract bool WinsWith(IFaction other);

    protected abstract bool IsAlliedWith(IFaction other);

    public T InnerFaction() => instance;

    public abstract bool IsAllied(IFaction<T> sameFaction);

    public abstract bool IsAllied(ISubFaction<T> subFaction);

    public ISubFaction<T>[] SubFactions() => subFactions;

    public override string ToString() => Name();
}*/
using System;
using System.Collections.Generic;
using TOHTOR.API.Reactive;

namespace TOHTOR.API.Stats;

// ReSharper disable once InconsistentNaming
public interface Statistics
{
    private static Func<Statistics> _globalManagerSupplier = () => new DefaultStatisticManager("Statistics.json");
    private static Statistics? currentStats;

    public static Statistics Current() => currentStats ??= _globalManagerSupplier();

    static Statistics()
    {
        Hooks.GameStateHooks.GameStartHook.Bind(nameof(Statistic), _ => currentStats = _globalManagerSupplier());
    }

    public static void SetGlobalStatisticManager(Func<Statistics> statistics) => _globalManagerSupplier = statistics;

    public void Track(Statistic statistic);

    public List<Statistic> GetAllStats();

    public Statistic<T> GetStat<T>(string identifier);

    public T? GetValue<T>(UniquePlayerId playerId, string identifier);

    public T? GetValue<T>(byte playerId, string identifier)
    {
        return GetValue<T>(UniquePlayerId.From(playerId), identifier);
    }
}
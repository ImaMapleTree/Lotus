using System;
using System.Collections.Generic;
using Lotus.API.Player;

namespace Lotus.API.Stats;

// ReSharper disable once InconsistentNaming
public interface Statistic<T> : Statistic
{
    public static Statistic<T> Create(string identifier, Func<string> name, T? defaultValue = default, bool accumulative = false)
    {
        return accumulative ? new AccumulatingStatistic<T>(identifier, name, defaultValue) : new BoundStatistic<T>(identifier, name, defaultValue);
    }

    public static IAccumulativeStatistic<T> CreateAccumulative(string identifier, Func<string> name, T? defaultValue = default)
    {
        return new AccumulatingStatistic<T>(identifier, name, defaultValue);
    }

    public void AddStatConsumer(Action<UniquePlayerId, Statistic<T>> consumer);

    public T? GetValue(UniquePlayerId playerId);

    object? Statistic.GetGenericValue(UniquePlayerId playerId) => GetValue(playerId);

    public T? GetValue(byte playerId);

    object? Statistic.GetGenericValue(byte playerId) => GetValue(playerId);

    public T? this[UniquePlayerId playerId] { get; set; }

    public T? this[byte playerId] { get; set; }

    public void Update(UniquePlayerId playerId, T value);

    public void Update(UniquePlayerId playerId, Func<T, T> updateFunc);

    public void Update(byte playerId, T value);

    public void Update(byte playerId, Func<T, T> updateFunc);
}

public interface IAccumulativeStatistic<T> : Statistic<T>, IAccumulativeStatistic
{
    public T? AccumulatedValue(UniquePlayerId playerId);

    object? IAccumulativeStatistic.GenericAccumulativeValue(UniquePlayerId playerId) => AccumulatedValue(playerId);

    public T? AccumulatedValue(byte playerId);

    object? IAccumulativeStatistic.GenericAccumulativeValue(byte playerId) => AccumulatedValue(playerId);

    public void SetAccumulator(Func<T?, T?, T?> biFunction);
}

public interface IJsonStats: IPersistentStatistic
{
    public void FromJsonDict(Dictionary<string, string> stats);
    public Dictionary<string, string> ToJsonDict();
}

public interface IAccumulativeStatistic : IPersistentStatistic
{
    public object? GenericAccumulativeValue(UniquePlayerId playerId);
    public object? GenericAccumulativeValue(byte playerId);
}

public interface IPersistentStatistic : Statistic
{
}

// ReSharper disable once InconsistentNaming
public interface Statistic
{
    public string Identifier();

    public string Name();

    public object? GetGenericValue(byte playerId);

    public object? GetGenericValue(UniquePlayerId playerId);
}
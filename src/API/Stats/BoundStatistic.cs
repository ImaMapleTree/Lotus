using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHTOR.API.Reactive;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

namespace TOHTOR.API.Stats;

internal class BoundStatistic<T> : Statistic<T>, IPersistentStatistic
{
    private readonly string identifier;
    private string name;
    private List<Action<UniquePlayerId, Statistic<T>>> eventConsumers = new();

    protected readonly T? DefaultValue;
    protected readonly Dictionary<UniquePlayerId, T?> PlayerData = new();
    protected readonly string HookKey;


    public BoundStatistic(string identifier, string name, T? defaultValue)
    {
        this.identifier = identifier;
        this.name = name;
        this.DefaultValue = defaultValue;
        HookKey = $"Statistic::{identifier}";
        VentLogger.Trace($"Binding Statistic: {HookKey}");
        Hooks.GameStateHooks.GameStartHook.Bind(HookKey, _ => PlayerData.Clear());
    }

    public void AddStatConsumer(Action<UniquePlayerId, Statistic<T>> consumer) => eventConsumers.Add(consumer);

    public string Identifier() => identifier;

    public T? GetValue(UniquePlayerId playerId) => PlayerData.GetOrCompute(playerId, () => DefaultValue);

    public T? GetValue(byte playerId) => GetValue(UniquePlayerId.From(playerId));

    public string Name() => name;

    public T? this[UniquePlayerId playerId]
    {
        get => GetValue(playerId);
        set => Update(playerId, value!);
    }

    public T? this[byte playerId]
    {
        get => GetValue(playerId);
        set => Update(playerId, value!);
    }

    public void Update(UniquePlayerId playerId, T value)
    {
        PlayerData[playerId] = value;
        eventConsumers.ForEach(ec => ec(playerId, this));
    }

    public void Update(UniquePlayerId playerId, Func<T, T> updateFunc) => Update(playerId, updateFunc(GetValue(playerId)!));

    public void Update(byte playerId, T value) => Update(UniquePlayerId.From(playerId), value);

    public void Update(byte playerId, Func<T, T> updateFunc) => Update(playerId, updateFunc(GetValue(playerId)!));

    public override bool Equals(object? obj)
    {
        return obj is Statistic statistic && statistic.Identifier().Equals(identifier);
    }

    public override int GetHashCode() => identifier.GetHashCode();

    public override string ToString()
    {
        return $"Statistic({Identifier()} ({PlayerData.Select(kv => $"{kv.Key}:{kv.Value}").Join()}))";
    }
}
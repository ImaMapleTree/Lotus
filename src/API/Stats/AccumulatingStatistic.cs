using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Player;
using Lotus.API.Reactive;
using VentLib.Logging;
using VentLib.Options.Processors;
using VentLib.Utilities.Extensions;

namespace Lotus.API.Stats;

internal class AccumulatingStatistic<T> : BoundStatistic<T>, IAccumulativeStatistic<T>, IJsonStats
{
    private Dictionary<string, T?> accumulatedData = new();
    private Func<T?, T?, T?> accumulator;

    public AccumulatingStatistic(string identifier, Func<string> name, T? defaultValue) : base(identifier, name, defaultValue)
    {

        Hooks.GameStateHooks.GameEndHook.Bind(HookKey, _ => MergeStatistics(), true);
        SetAccumulator(defaultValue);
    }

    private void MergeStatistics()
    {
        PlayerData.ForEach(kv => Merge(accumulatedData, kv));
    }

    private void Merge(Dictionary<string, T?> mergeDict, KeyValuePair<UniquePlayerId, T?> valuePair)
    {
        string key = valuePair.Key.ToString();
        T? value = valuePair.Value;
        T? currentValue = mergeDict.GetValueOrDefault(key);
        if (value == null) return;
        if (currentValue == null)
        {
            mergeDict[key] = value;
            return;
        }
        mergeDict[key] = accumulator(value, currentValue);
    }

    public T? AccumulatedValue(UniquePlayerId playerId)
    {
        Dictionary<string, T?> temporaryDict = new(accumulatedData);
        PlayerData.ForEach(kv => Merge(temporaryDict, kv));
        return temporaryDict.GetValueOrDefault(playerId.ToString(), DefaultValue);
    }

    public T? AccumulatedValue(byte playerId) => AccumulatedValue(UniquePlayerId.From(playerId));

    public void SetAccumulator(Func<T?, T?, T?> biFunction)
    {
        accumulator = biFunction;
    }

    public void FromJsonDict(Dictionary<string, string> dictionary)
    {
        accumulatedData = dictionary
            .Select(kv => (kv.Key, ValueTypeProcessors.ReadFromLine(kv.Value, typeof(T))))
            .ToDict(tuple => tuple.Key, tuple => (T?)tuple.Item2);
    }

    public Dictionary<string, string> ToJsonDict()
    {
        Dictionary<string, T?> temporaryDict = new(accumulatedData);
        PlayerData.ForEach(kv => Merge(temporaryDict, kv));

        return temporaryDict
            .Select(kv => (kv.Key, ValueTypeProcessors.WriteToString(kv.Value!)))
            .ToDict(tuple => tuple.Key, tuple => tuple.Item2);
    }

    private void SetAccumulator(T? defaultValue)
    {
        accumulator = (defaultValue) switch
        {
            byte => (t1, t2) =>
            {
                byte value = ByteAccumulator(t1, t2);
                return value is T t ? t : default;
            },
            sbyte => (t1, t2) =>
            {
                sbyte value = SByteAccumulator(t1, t2);
                return value is T t ? t : default;
            },
            short => (t1, t2) =>
            {
                short value = ShortAccumulator(t1, t2);
                return value is T t ? t : default;
            },
            ushort => (t1, t2) =>
            {
                ushort value = UShortAccumulator(t1, t2);
                return value is T t ? t : default;
            },
            int => (t1, t2) =>
            {
                int value = IntAccumulator(t1, t2);
                return value is T t ? t : default;
            },
            uint => (t1, t2) =>
            {
                uint value = UIntAccumulator(t1, t2);
                return value is T t ? t : default;
            },
            long => (t1, t2) =>
            {
                long value = LongAccumulator(t1, t2);
                return value is T t ? t : default;
            },
            ulong => (t1, t2) =>
            {
                ulong value = ULongAccumulator(t1, t2);
                return value is T t ? t : default;
            },
            float => (t1, t2) =>
            {
                float value = FloatAccumulator(t1, t2);
                return value is T t ? t : default;
            },
            double => (t1, t2) =>
            {
                double value = DoubleAccumulator(t1, t2);
                return value is T t ? t : default;
            },
            decimal => (t1, t2) =>
            {
                decimal value = DecimalAccumulator(t1, t2);
                return value is T t ? t : default;
            },
            _ => (t1, t2) =>
            {
                VentLogger.Fatal($"Using Default Accumulator: T1 = {t1}, T2 = {t2}");
                return t1;
            }
        };
    }

    private static byte ByteAccumulator(T? t1, T? t2) => t1 is byte b1 ? (byte)(t2 is byte b2 ? b1 + b2 : b1) : default;
    private static sbyte SByteAccumulator(T? t1, T? t2) => t1 is sbyte b1 ? (sbyte)(t2 is sbyte b2 ? b1 + b2 : b1) : default;
    private static short ShortAccumulator(T? t1, T? t2) => t1 is short b1 ? (short)(t2 is short b2 ? b1 + b2 : b1) : default;
    private static ushort UShortAccumulator(T? t1, T? t2) => t1 is ushort b1 ? (ushort)(t2 is ushort b2 ? b1 + b2 : b1) : default;
    private static int IntAccumulator(T? t1, T? t2) => t1 is int b1 ? t2 is int b2 ? b1 + b2 : b1 : default;
    private static uint UIntAccumulator(T? t1, T? t2) => t1 is uint b1 ? t2 is uint b2 ? b1 + b2 : b1 : default;
    private static long LongAccumulator(T? t1, T? t2) => t1 is long b1 ? t2 is long b2 ? b1 + b2 : b1 : default;
    private static ulong ULongAccumulator(T? t1, T? t2) => t1 is ulong b1 ? t2 is ulong b2 ? b1 + b2 : b1 : default;
    private static float FloatAccumulator(T? t1, T? t2) => t1 is float b1 ? t2 is float b2 ? b1 + b2 : b1 : default;
    private static double DoubleAccumulator(T? t1, T? t2) => t1 is double b1 ? t2 is double b2 ? b1 + b2 : b1 : default;
    private static decimal DecimalAccumulator(T? t1, T? t2) => t1 is decimal b1 ? t2 is decimal b2 ? b1 + b2 : b1 : default;
}
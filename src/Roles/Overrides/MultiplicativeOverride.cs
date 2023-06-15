using System;
using AmongUs.GameOptions;
using Lotus.Logging;

namespace Lotus.Roles.Overrides;

public class MultiplicativeOverride: GameOptionOverride
{
    private IGameOptions? lastOption;

    public MultiplicativeOverride(Override option, object? value, Func<bool>? condition = null) : base(option, value, condition)
    {
    }

    public MultiplicativeOverride(Override option, Func<object> valueSupplier, Func<bool>? condition = null) : base(option, valueSupplier, condition)
    {
    }

    public override void ApplyTo(IGameOptions options)
    {
        if (!Condition?.Invoke() ?? false) return;
        if (ReferenceEquals(options, lastOption)) return;
        lastOption = options;

        object value = Option.GetValue(options);
        DevLogger.Log($"Current option value: {value}");
        object newValue = Option.SetValue(options, Multiply(value));
        DevLogger.Log($"New option value: {newValue}");
    }

    private object? Multiply(dynamic? originalValue)
    {
        dynamic? myValue = GetValue();
        if (myValue == null) return originalValue;
        try
        {
            return originalValue * myValue;
        }
        catch
        {
            return originalValue;
        }
    }

    public override string ToString()
    {
        return $"MultiplicativeOverride(override={Option}, value={GetValue()})";
    }
}
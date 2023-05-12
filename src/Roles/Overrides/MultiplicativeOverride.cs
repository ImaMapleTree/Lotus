using System;
using AmongUs.GameOptions;
using TOHTOR.Logging;

namespace TOHTOR.Roles.Overrides;

public class MultiplicativeOverride: GameOptionOverride
{
    public MultiplicativeOverride(Override option, object? value, Func<bool>? condition = null) : base(option, value, condition)
    {
    }

    public MultiplicativeOverride(Override option, Func<object> valueSupplier, Func<bool>? condition = null) : base(option, valueSupplier, condition)
    {
    }
    
    public override void ApplyTo(IGameOptions options)
    {
        if (!Condition?.Invoke() ?? false) return;
        object value = Option.GetValue(options);
        DevLogger.Log($"OLD VALUE: {value}");
        DevLogger.Log($"NEW VALUE: {Option.SetValue(options, Multiply(value))})");
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
}
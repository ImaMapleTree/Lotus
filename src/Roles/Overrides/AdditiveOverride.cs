using System;
using AmongUs.GameOptions;
using Lotus.Logging;

namespace Lotus.Roles.Overrides;

public class AdditiveOverride: GameOptionOverride
{
    private IGameOptions? lastOption;

    public AdditiveOverride(Override option, object? value, Func<bool>? condition = null) : base(option, value, condition)
    {
    }

    public AdditiveOverride(Override option, Func<object> valueSupplier, Func<bool>? condition = null) : base(option, valueSupplier, condition)
    {
    }

    public override void ApplyTo(IGameOptions options)
    {
        if (!Condition?.Invoke() ?? false) return;
        if (ReferenceEquals(lastOption, options)) return;
        lastOption = options;

        object value = Option.GetValue(options);
        Option.SetValue(options, Add(value));
    }

    private object? Add(dynamic? originalValue)
    {
        dynamic? myValue = GetValue();
        if (myValue == null) return originalValue;
        try
        {
            return originalValue + myValue;
        }
        catch
        {
            return originalValue;
        }
    }
}
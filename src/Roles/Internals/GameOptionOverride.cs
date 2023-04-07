#nullable enable
using System;
using AmongUs.GameOptions;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Options;
using VentLib.Logging;

namespace TOHTOR.Roles.Internals;

public class GameOptionOverride
{
    public readonly Override Option;
    private readonly object? value;
    private readonly Func<object>? supplier;
    private readonly Func<bool>? condition;

    public GameOptionOverride(Override option, object? value, Func<bool>? condition = null)
    {
        this.Option = option;
        this.value = value;
        this.condition = condition;
    }

    public GameOptionOverride(Override option, Func<object> valueSupplier, Func<bool>? condition = null)
    {
        this.Option = option;
        this.supplier = valueSupplier;
        this.condition = condition;
    }


    public void ApplyTo(IGameOptions options)
    {
        if (condition != null && !condition.Invoke()) return;
        switch (Option)
        {
            case Override.AnonymousVoting:
                options.SetBool(BoolOptionNames.AnonymousVotes, (bool)(GetValue() ?? OriginalOptions.AnonymousVotes()));
                break;
            case Override.DiscussionTime:
                options.SetInt(Int32OptionNames.DiscussionTime, (int)(GetValue() ?? OriginalOptions.DiscussionTime()));
                break;
            case Override.VotingTime:
                options.SetInt(Int32OptionNames.VotingTime, (int)(GetValue() ?? OriginalOptions.VotingTime()));
                break;
            case Override.PlayerSpeedMod:
                options.SetFloat(FloatOptionNames.PlayerSpeedMod, (float)(GetValue() ?? OriginalOptions.PlayerSpeedMod()));
                break;
            case Override.CrewLightMod:
                options.SetFloat(FloatOptionNames.CrewLightMod, (float)(GetValue() ?? OriginalOptions.CrewLightMod()));
                break;
            case Override.ImpostorLightMod:
                options.SetFloat(FloatOptionNames.ImpostorLightMod, (float)(GetValue() ?? OriginalOptions.ImpostorLightMod()));
                break;
            case Override.KillCooldown:
                options.SetFloat(FloatOptionNames.KillCooldown, (float)(GetValue() ?? OriginalOptions.KillCooldown()));
                break;
            case Override.ShapeshiftDuration:
                options.SetFloat(FloatOptionNames.ShapeshifterDuration, (float)(GetValue() ??OriginalOptions.ShapeshifterDuration()));
                break;
            case Override.ShapeshiftCooldown:
                options.SetFloat(FloatOptionNames.ShapeshifterCooldown, (float)(GetValue() ?? OriginalOptions.ShapeshifterCooldown()));
                break;
            case Override.GuardianAngelDuration:
                options.SetFloat(FloatOptionNames.ProtectionDurationSeconds, (float)(GetValue() ?? OriginalOptions.ProtectionDurationSeconds()));
                break;
            case Override.GuardianAngelCooldown:
                options.SetFloat(FloatOptionNames.GuardianAngelCooldown, (float)(GetValue() ?? OriginalOptions.GuardianAngelCooldown()));
                break;
            case Override.KillDistance:
                options.SetInt(Int32OptionNames.KillDistance, (int)(GetValue() ?? OriginalOptions.KillDistance()));
                break;
            case Override.EngVentCooldown:
                options.SetFloat(FloatOptionNames.EngineerCooldown, (float)(GetValue() ?? OriginalOptions.EngineerCooldown()));
                break;
            case Override.EngVentDuration:
                options.SetFloat(FloatOptionNames.EngineerInVentMaxTime, (float)(GetValue() ?? OriginalOptions.EngineerInVentMaxTime()));
                break;
            case Override.CanUseVent:
            default:
                VentLogger.Warn($"Invalid Option Override: {this}", "ApplyOverride");
                break;
        }
    }

    private object? GetValue() => supplier == null ? value : supplier.Invoke();

    public override bool Equals(object? obj)
    {
        if (obj is not GameOptionOverride @override) return false;
        return @override.Option == this.Option;
    }

    public override int GetHashCode()
    {
        return this.Option.GetHashCode();
    }

    public override string ToString()
    {
        return $"GameOptionOverride(override={Option}, value={value})";
    }
}

public enum Override
{
    // Role overrides
    CanUseVent,


    // Game override
    AnonymousVoting,
    DiscussionTime,
    VotingTime,
    PlayerSpeedMod,
    CrewLightMod,
    ImpostorLightMod,
    KillCooldown,
    KillDistance,

    // Role specific overrides
    ShapeshiftDuration,
    ShapeshiftCooldown,

    GuardianAngelDuration,
    GuardianAngelCooldown,

    EngVentCooldown,
    EngVentDuration,
}

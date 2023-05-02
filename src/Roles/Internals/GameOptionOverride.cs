#nullable enable
using System;
using AmongUs.GameOptions;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Options;
using UnityEngine;
using VentLib.Logging;

namespace TOHTOR.Roles.Internals;

public class GameOptionOverride
{
    public readonly Override Option;
    private readonly object? value;
    private readonly Func<object>? supplier;
    private readonly Func<bool>? condition;

    private object? _debugValue;

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


    // TODO figure out applyto
    public void ApplyTo(IGameOptions options)
    {
        if (condition != null && !condition.Invoke()) return;

        switch (Option)
        {
            case Override.AnonymousVoting:
                options.SetBool(BoolOptionNames.AnonymousVotes, (bool)(GetValue() ?? AUSettings.AnonymousVotes()));
                break;
            case Override.DiscussionTime:
                options.SetInt(Int32OptionNames.DiscussionTime, (int)(GetValue() ?? AUSettings.DiscussionTime()));
                break;
            case Override.VotingTime:
                options.SetInt(Int32OptionNames.VotingTime, (int)(GetValue() ?? AUSettings.VotingTime()));
                break;
            case Override.PlayerSpeedMod:
                options.SetFloat(FloatOptionNames.PlayerSpeedMod, (float)(GetValue() ?? AUSettings.PlayerSpeedMod()));
                break;
            case Override.CrewLightMod:
                options.SetFloat(FloatOptionNames.CrewLightMod, (float)(GetValue() ?? AUSettings.CrewLightMod()));
                break;
            case Override.ImpostorLightMod:
                options.SetFloat(FloatOptionNames.ImpostorLightMod, (float)(GetValue() ?? AUSettings.ImpostorLightMod()));
                break;
            case Override.KillCooldown:
                options.SetFloat(FloatOptionNames.KillCooldown, Mathf.Clamp((float)(GetValue() ?? AUSettings.KillCooldown()), 0.1f, float.MaxValue));
                break;
            case Override.ShapeshiftDuration:
                options.SetFloat(FloatOptionNames.ShapeshifterDuration, (float)(GetValue() ??AUSettings.ShapeshifterDuration()));
                break;
            case Override.ShapeshiftCooldown:
                options.SetFloat(FloatOptionNames.ShapeshifterCooldown, (float)(GetValue() ?? AUSettings.ShapeshifterCooldown()));
                break;
            case Override.GuardianAngelDuration:
                options.SetFloat(FloatOptionNames.ProtectionDurationSeconds, (float)(GetValue() ?? AUSettings.ProtectionDurationSeconds()));
                break;
            case Override.GuardianAngelCooldown:
                options.SetFloat(FloatOptionNames.GuardianAngelCooldown, (float)(GetValue() ?? AUSettings.GuardianAngelCooldown()));
                break;
            case Override.KillDistance:
                options.SetInt(Int32OptionNames.KillDistance, (int)(GetValue() ?? AUSettings.KillDistance()));
                break;
            case Override.EngVentCooldown:
                options.SetFloat(FloatOptionNames.EngineerCooldown, (float)(GetValue() ?? AUSettings.EngineerCooldown()));
                break;
            case Override.EngVentDuration:
                options.SetFloat(FloatOptionNames.EngineerInVentMaxTime, (float)(GetValue() ?? AUSettings.EngineerInVentMaxTime()));
                break;
            case Override.CanUseVent:
            default:
                VentLogger.Warn($"Invalid Option Override: {this}", "ApplyOverride");
                break;
        }

        VentLogger.Trace($"Applying Override: {Option} => {_debugValue}", "Override::ApplyTo");
    }

    public object? GetValue() => _debugValue = supplier == null ? value : supplier.Invoke();

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

#nullable enable
using System;
using AmongUs.GameOptions;
using TOHTOR.API;
using UnityEngine;
using VentLib.Logging;

namespace TOHTOR.Roles.Overrides;

public class GameOptionOverride
{
    public readonly Override Option;
    protected readonly Func<bool>? Condition;
    private readonly object? value;
    private readonly Func<object>? supplier;

    protected object? DebugValue;

    public GameOptionOverride(Override option, object? value, Func<bool>? condition = null)
    {
        this.Option = option;
        this.value = value;
        this.Condition = condition;
    }

    public GameOptionOverride(Override option, Func<object> valueSupplier, Func<bool>? condition = null)
    {
        this.Option = option;
        this.supplier = valueSupplier;
        this.Condition = condition;
    }


    // TODO figure out applyto
    public virtual void ApplyTo(IGameOptions options)
    {
        if (!Condition?.Invoke() ?? false) return;

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
                options.SetFloat(FloatOptionNames.PlayerSpeedMod, Mathf.Clamp((float)(GetValue() ?? AUSettings.PlayerSpeedMod()), 0 , ModConstants.MaxPlayerSpeed));
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

        VentLogger.Trace($"Applying Override: {Option} => {DebugValue}", "Override::ApplyTo");
    }

    public virtual object? GetValue() => DebugValue = supplier == null ? value : supplier.Invoke();

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
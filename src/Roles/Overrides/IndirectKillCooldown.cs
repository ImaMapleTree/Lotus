using System;
using System.Diagnostics;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.Logging;

namespace Lotus.Roles.Overrides;

public class IndirectKillCooldown: GameOptionOverride
{
    private string hookKey;
    private Func<float> cooldownSupplier;
    private bool doubled;

    /// <summary>
    /// Creates a <see cref="GameOptionOverride"/> that overrides kill cooldown
    /// specifically for killing roles that do not directly kill their target. (Ex: Vampire)
    /// </summary>
    /// <param name="expectedCooldown">the role's expected kill cooldown (not multiplied)</param>
    /// <param name="condition">a conditional function that determines if the override should be applied</param>
    public IndirectKillCooldown(float expectedCooldown, Func<bool>? condition = null) : this(() => expectedCooldown, condition)
    {
    }

    /// <summary>
    /// Creates a <see cref="GameOptionOverride"/> that overrides kill cooldown
    /// specifically for killing roles that do not directly kill their target. (Ex: Vampire)
    /// </summary>
    /// <param name="expectedCooldown">the role's expected kill cooldown (not multiplied)</param>
    /// <param name="condition">a conditional function that determines if the override should be applied</param>
    public IndirectKillCooldown(Func<float> expectedCooldown, Func<bool>? condition = null) : base(Override.KillCooldown, () => expectedCooldown(), condition)
    {
        hookKey = $"{nameof(IndirectKillCooldown)}~{Game.NextMatchID()}";
        cooldownSupplier = expectedCooldown;
        DevLogger.Log($"Binding: {hookKey}");
        Hooks.GameStateHooks.RoundStartHook.Bind(hookKey, _ =>
        {
            DevLogger.Log($"DOUBLED {hookKey}");
            doubled = true;
        }, true);
        Hooks.GameStateHooks.RoundEndHook.Bind(hookKey, _ => doubled = false, true);
    }

    public override object? GetValue()
    {
        float cooldown = cooldownSupplier();
        return DebugValue = doubled ? cooldown * 2 : cooldown;
    }
}
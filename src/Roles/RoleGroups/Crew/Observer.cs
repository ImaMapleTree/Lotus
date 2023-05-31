using Lotus.API;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Patches.Systems;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Crew;

public class Observer: Crewmate
{
    private bool slowlyGainsVision;
    private float visionGain;
    private bool overrideStartingVision;
    private float startingVision;
    private float totalVisionMod;

    private float currentVisionMod;
    private bool sabotageImmunity;

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        currentVisionMod = overrideStartingVision ? startingVision : AUSettings.CrewLightMod();
    }

    protected override void OnTaskComplete(Optional<NormalPlayerTask> _)
    {
        if (slowlyGainsVision)
            currentVisionMod = Mathf.Clamp(currentVisionMod + visionGain, 0, totalVisionMod);
        if (HasAllTasksComplete)
            currentVisionMod = totalVisionMod;
        SyncOptions();
    }

    [RoleAction(RoleActionType.SabotageStarted)]
    [RoleAction(RoleActionType.SabotageFixed)]
    private void AdjustSabotageVision(ActionHandle handle)
    {
        VentLogger.Trace($"Fixing Player Vision (HasAllTasksComplete = {HasAllTasksComplete}, SabotageImmune = {sabotageImmunity})", "Observer");
        Async.Schedule(SyncOptions, handle.ActionType is RoleActionType.SabotageStarted ? 4f : 0.2f);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Slowly Gains Vision")
                .BindBool(v => slowlyGainsVision = v)
                .AddOnOffValues(false)
                .ShowSubOptionPredicate(v => (bool)v)
                .SubOption(sub2 => sub2
                    .Name("Vision Gain On Task Complete")
                    .BindFloat(v => visionGain = v)
                    .AddFloatRange(0.05f, 1, 0.05f, 2, "x").Build())
                .Build())
            .SubOption(sub => sub
                .Name("Override Starting Vision")
                .BindBool(v => overrideStartingVision = v)
                .ShowSubOptionPredicate(v => (bool)v)
                .AddOnOffValues(false)
                .SubOption(sub2 => sub2
                    .Name("Starting Vision Modifier")
                    .BindFloat(v => startingVision = v)
                    .AddFloatRange(0.25f, 2, 0.25f, 0, "x").Build())
                .Build())
            .SubOption(sub => sub
                .Name("Finished Tasks Vision")
                .BindFloat(v => totalVisionMod = v)
                .AddFloatRange(0.25f, 5f, 0.25f, 8, "x").Build())
            .SubOption(sub => sub
                .Name("Lights Immunity If Tasks Finished")
                .BindBool(v => sabotageImmunity = v)
                .AddOnOffValues().Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.93f, 0.9f, 0.75f))
            .OptionOverride(Override.CrewLightMod, () => currentVisionMod)
            .OptionOverride(Override.CrewLightMod, () => currentVisionMod * 5,
                () => sabotageImmunity && HasAllTasksComplete && SabotagePatch.CurrentSabotage != null && SabotagePatch.CurrentSabotage.SabotageType() is SabotageType.Lights);
}
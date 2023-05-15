using Lotus.API;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Crew;

public class Speedrunner : Crewmate
{
    private bool speedBoostOnTaskComplete;
    private float smallRewardBoost;
    private float smalRewardDuration;

    private int tasksUntilSpeedBoost;
    private bool slowlyAcquireSpeedBoost;
    private float speedBoostGain;

    private float totalSpeedBoost;

    private float currentSpeedBoost;

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        currentSpeedBoost = AUSettings.PlayerSpeedMod();
    }

    protected override void OnTaskComplete(Optional<NormalPlayerTask> _)
    {
        if (slowlyAcquireSpeedBoost)
            currentSpeedBoost = Mathf.Clamp(currentSpeedBoost + speedBoostGain, 0, totalSpeedBoost);
        if (TasksComplete >= tasksUntilSpeedBoost)
            currentSpeedBoost = totalSpeedBoost;
        if (speedBoostOnTaskComplete)
        {
            currentSpeedBoost += smallRewardBoost;
            Async.Schedule(() =>
            {
                currentSpeedBoost -= smallRewardBoost;
                this.SyncOptions();
            }, smalRewardDuration);
        }

        SyncOptions();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Small Boost When Finishing a Task")
                .Bind(v => speedBoostOnTaskComplete = (bool)v)
                .ShowSubOptionPredicate(v => (bool)v)
                .AddOnOffValues(false)
                .SubOption(sub2 => sub2
                    .Name("Temporary Speed Boost")
                    .Bind(v => smallRewardBoost = (float)v)
                    .AddFloatRange(0.1f, 1f, 0.05f, 1, "x")
                    .Build())
                .SubOption(sub2 => sub2
                    .Name("Temporary Boost Duration")
                    .Bind(v => smalRewardDuration = (float)v)
                    .AddFloatRange(2f, 12f, 0.5f, 2, "s")
                    .Build())
                .Build())
            .SubOption(sub => sub
                .Name("Tasks Until Speed Boost")
                .Bind(v => tasksUntilSpeedBoost = (int)v)
                .AddIntRange(1, 20, 1, 5)
                .Build())
            .SubOption(sub => sub
                .Name("Slowly Gain Speed Boost")
                .Bind(v => slowlyAcquireSpeedBoost = (bool)v)
                .ShowSubOptionPredicate(v => (bool)v)
                .AddOnOffValues(false)
                .SubOption(sub2 => sub2
                    .Name("Permanent Gain")
                    .Bind(v => speedBoostGain = (float)v)
                    .AddFloatRange(0.1f, 1f, 0.1f, 1, "x")
                    .Build())
                .Build())
            .SubOption(sub => sub
                .Name("Final Speed Boost")
                .Bind(v => totalSpeedBoost = (float)v)
                .AddFloatRange(0.5f, 3f, 0.25f, 7, "x")
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.4f, 0.17f, 0.93f)).OptionOverride(Override.PlayerSpeedMod, () => currentSpeedBoost);
}


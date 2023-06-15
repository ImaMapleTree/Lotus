using System;
using Lotus.API;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Crew;

public class Speedrunner : Crewmate
{
    private float temporarySpeedReward;
    private float smalRewardDuration;

    private int tasksUntilSpeedBoost;
    private float permanentSpeedGain;

    private float totalSpeedBoost;

    private float currentSpeedBoost;

    protected override void PostSetup() => currentSpeedBoost = AUSettings.PlayerSpeedMod();

    protected override void OnTaskComplete(Optional<NormalPlayerTask> _)
    {
        if (permanentSpeedGain > 0f)
            currentSpeedBoost = Mathf.Clamp(currentSpeedBoost + permanentSpeedGain, 0, totalSpeedBoost);

        if (tasksUntilSpeedBoost != -1 && (TotalTasks - TasksComplete) < tasksUntilSpeedBoost)
            currentSpeedBoost = Math.Max(currentSpeedBoost, totalSpeedBoost);

        if (temporarySpeedReward > 0f)
        {
            currentSpeedBoost += temporarySpeedReward;
            Async.Schedule(() =>
            {
                currentSpeedBoost -= temporarySpeedReward;
                this.SyncOptions();
            }, smalRewardDuration);
        }

        SyncOptions();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Temporary Boost Upon Finishing Task", Translations.Options.TempBoostOnTaskFinish)
                .Value(v => v.Text(GeneralOptionTranslations.DisabledText).Color(Color.red).Value(0f).Build())
                .AddFloatRange(0.1f, 0.5f, 0.05f, 1, "x")
                .BindFloat(f => temporarySpeedReward = f)
                .ShowSubOptionPredicate(f => (float)f > 0)
                .SubOption(sub2 => sub2
                    .KeyName("Temporary Boost Duration", Translations.Options.TempBoostDuration)
                    .AddFloatRange(1f, 12f, 0.5f, 4, GeneralOptionTranslations.SecondsSuffix)
                    .BindFloat(f => smalRewardDuration = f)
                    .Build())
                .Build())
            .SubOption(sub => sub
                .KeyName("Permanent Speed Gain per Task", Translations.Options.PermanentSpeedGain)
                .Value(v => v.Text(GeneralOptionTranslations.DisabledText).Color(Color.red).Value(0f).Build())
                .AddFloatRange(0.1f, 1f, 0.05f, 1, "x")
                .BindFloat(f => permanentSpeedGain = f)
                .Build())
            .SubOption(sub => sub
                .KeyName("Tasks Remaining Until Final Speed Boost", Translations.Options.TaskUntilLargeSpeedBoost)
                .Value(v => v.Value(-1).Text(GeneralOptionTranslations.DisabledText).Color(Color.red).Build())
                .AddIntRange(0, 20, defaultIndex: 3)
                .BindInt(i => tasksUntilSpeedBoost = i)
                .ShowSubOptionPredicate(i => (int)i != -1)
                .SubOption(sub2 => sub2
                    .KeyName("Final Speed Boost", Translations.Options.FinalSpeedBoost)
                    .AddFloatRange(0.5f, 3f, 0.25f, 7, "x")
                    .BindFloat(f => totalSpeedBoost = f)
                    .Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.4f, 0.17f, 0.93f))
            .OptionOverride(Override.PlayerSpeedMod, () => currentSpeedBoost);

    [Localized(nameof(Speedrunner))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(TempBoostOnTaskFinish))]
            public static string TempBoostOnTaskFinish = "Temporary Boost Upon Finishing Task";

            [Localized(nameof(TempBoostDuration))]
            public static string TempBoostDuration = "Temporary Boost Duration";

            [Localized(nameof(PermanentSpeedGain))]
            public static string PermanentSpeedGain = "Permanent Speed Gain per Task";

            [Localized(nameof(TaskUntilLargeSpeedBoost))]
            public static string TaskUntilLargeSpeedBoost = "Task Remaining Until Main Speed Boost";

            [Localized(nameof(FinalSpeedBoost))]
            public static string FinalSpeedBoost = "Final Speed Boost";
        }
    }
}


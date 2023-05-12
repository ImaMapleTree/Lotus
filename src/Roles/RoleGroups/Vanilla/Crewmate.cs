using System;
using AmongUs.GameOptions;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Interfaces;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Vanilla;

public class Crewmate : CustomRole, IOverridenTaskHolderRole
{
    public int TotalTasks => initialized ? tasks : taskSupplier?.Invoke() ?? 0;
    public int TasksComplete;
    public bool HasAllTasksDone => TasksComplete >= TotalTasks;

    public bool HasOverridenTasks;
    public bool HasCommonTasks;
    public int ShortTasks;
    public int LongTasks;

    private int tasks;
    private Func<int>? taskSupplier;
    private bool initialized;

    [UIComponent(UI.Counter, ViewMode.Overriden, GameState.InMeeting, GameState.Roaming)]
    protected string TaskTracker() => RealRole.IsImpostor() ? "" : RoleUtils.Counter(TasksComplete, TotalTasks);

    [RoleAction(RoleActionType.RoundStart, blockable: false, triggerAfterDeath: true)]
    protected void SetTaskTotal(bool gameStart)
    {
        if (!gameStart) return;
        initialized = true;
        tasks = taskSupplier?.Invoke() ?? 0;
    }

    [RoleAction(RoleActionType.TaskComplete, triggerAfterDeath: true, blockable: false)]
    protected void InternalTaskComplete(PlayerControl player)
    {
        if (player.PlayerId != MyPlayer.PlayerId) return;
        TasksComplete++;
        if (player.IsAlive()) this.OnTaskComplete();
        Game.MatchData.GameHistory.AddEvent(new TaskCompleteEvent(player));
    }

    public bool AssignCommonTasks() => HasCommonTasks;

    public int LongTaskAmount() => LongTasks;

    public int ShortTaskAmount() => ShortTasks;

    public bool OverrideTasks() => HasOverridenTasks;

    public virtual bool HasTasks() => true;

    public virtual bool TasksApplyToTotal() => true;

    /// <summary>
    /// Sets up the task counter for crewmate roles. If you extend this class and want this done automatically please call base.Setup()
    /// </summary>
    /// <param name="player">Player wrapped into this role's class instance</param>
    protected override void Setup(PlayerControl player) => taskSupplier = () => player.Data?.Tasks?.Count ?? 0;

    /// <summary>
    /// Called automatically when this player completes a task
    /// </summary>
    protected virtual void OnTaskComplete() { }

    protected GameOptionBuilder AddTaskOverrideOptions(GameOptionBuilder builder)
    {
        return builder.SubOption(sub => sub
            .KeyName($"Override {EnglishRoleName}'s Tasks", CrewmateTranslations.CrewmateOptionTranslations.OverrideRoleTasks.Formatted(RoleColor.Colorize(RoleName)))
            .Bind(v => HasOverridenTasks = (bool)v)
            .ShowSubOptionPredicate(v => (bool)v)
            .AddOnOffValues(false)
            .SubOption(sub2 => sub2
                .KeyName("Allow Common Tasks", CrewmateTranslations.CrewmateOptionTranslations.AllowCommonTasks)
                .Bind(v => HasCommonTasks = (bool)v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub2 => sub2
                .KeyName($"{EnglishRoleName} Long Tasks", CrewmateTranslations.CrewmateOptionTranslations.CustomLongTasks.Formatted(RoleColor.Colorize(RoleName)))
                .Bind(v => LongTasks = (int)v)
                .AddIntRange(0, 20, 1, 5)
                .Build())
            .SubOption(sub2 => sub2
                .KeyName($"{EnglishRoleName} Short Tasks", CrewmateTranslations.CrewmateOptionTranslations.CustomShortTasks.Formatted(RoleColor.Colorize(RoleName)))
                .Bind(v => ShortTasks = (int)v)
                .AddIntRange(1, 20, 1, 5)
                .IOSettings(io => io.UnknownValueAction = ADEAnswer.UseDefault)
                .Build())
            .Build());
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.VanillaRole(RoleTypes.Crewmate).Faction(FactionInstances.Crewmates).RoleColor("#b6f0ff");

    [Localized(nameof(Crewmate))]
    private static class CrewmateTranslations
    {
        [Localized("Options")]
        internal static class CrewmateOptionTranslations
        {
            [Localized(nameof(OverrideRoleTasks))]
            public static string OverrideRoleTasks = "Override {0}'s Tasks";

            [Localized(nameof(AllowCommonTasks))]
            public static string AllowCommonTasks = "Allow Common Tasks";

            [Localized(nameof(CustomLongTasks))]
            public static string CustomLongTasks = "{0} Long Tasks";

            [Localized(nameof(CustomShortTasks))]
            public static string CustomShortTasks = "{0} Short Tasks";
        }
    }
}


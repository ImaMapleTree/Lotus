using System;
using AmongUs.GameOptions;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Interfaces;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Options.Game;

namespace TOHTOR.Roles.RoleGroups.Vanilla;

public class Crewmate : CustomRole, IOverridenTaskHolderRole
{
    public int TotalTasks => taskSupplier?.Invoke() ?? 0;
    public int TasksComplete;
    public bool HasAllTasksDone => TasksComplete >= TotalTasks;

    // Used in subclasses but setup here
    public bool HasOverridenTasks;
    public bool HasCommonTasks;
    public int ShortTasks;
    public int LongTasks;

    private Func<int>? taskSupplier;

    // TODO: Maybe make color customizable idk that's pretty extreme
    [UIComponent(UI.Counter, ViewMode.Overriden, GameState.InMeeting, GameState.Roaming)]
    protected string TaskTracker() => RoleUtils.Counter(TasksComplete, TotalTasks);

    [RoleAction(RoleActionType.TaskComplete, triggerAfterDeath: true, blockable: false)]
    protected void InternalTaskComplete(PlayerControl player)
    {
        if (player.PlayerId != MyPlayer.PlayerId) return;
        TasksComplete++;
        this.OnTaskComplete();
        Game.GameHistory.AddEvent(new TaskCompleteEvent(player));
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
            .Name($"Override {RoleName}'s Tasks")
            .Bind(v => HasOverridenTasks = (bool)v)
            .ShowSubOptionPredicate(v => (bool)v)
            .AddOnOffValues(false)
            .SubOption(sub2 => sub2
                .Name("Allow Common Tasks")
                .Bind(v => HasCommonTasks = (bool)v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub2 => sub2
                .Name($"{RoleName} Long Tasks")
                .Bind(v => LongTasks = (int)v)
                .AddIntRange(0, 20, 1, 5)
                .Build())
            .SubOption(sub2 => sub2
                .Name($"{RoleName} Short Tasks")
                .Bind(v => ShortTasks = (int)v)
                .AddIntRange(1, 20, 1, 5)
                .Build())
            .Build());
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.VanillaRole(RoleTypes.Crewmate).Faction(FactionInstances.Crewmates).RoleColor("#b6f0ff");
}


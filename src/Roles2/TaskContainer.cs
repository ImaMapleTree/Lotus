using Lotus.API;

namespace Lotus.Roles2;

public class TaskContainer
{
    public static NamespacedKey<TaskContainer> Key = NamespacedKey.Lotus<TaskContainer>(nameof(TaskContainer));
    public static readonly TaskContainer None = new();

    public bool HasTasks { get; }
    public bool HasCommonTasks { get; }
    public bool TasksApplyToTotal { get; }
    public bool TasksOverrideDefaults { get; }

    public int TotalTasks => ShortTasks + CommonTasks + LongTasks;

    public int ShortTasks { get; }
    public int CommonTasks { get; }
    public int LongTasks { get; }
    public int TasksComplete { get; }
}
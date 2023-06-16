namespace Lotus.Roles.Interfaces;

public interface ITaskHolderRole
{
    /// <summary>
    /// If this role should be assigned tasks
    /// </summary>
    /// <returns>true if the role should be assigned tasks, false otherwise</returns>
    public bool HasTasks();

    /// <summary>
    /// If this role's task should count for the overall total game tasks
    /// </summary>
    /// <returns>true if the tasks should count to the total, otherwise false</returns>
    public bool TasksApplyToTotal();

    public int TotalTasks { get; }

    public int CompleteTasks { get; }
}
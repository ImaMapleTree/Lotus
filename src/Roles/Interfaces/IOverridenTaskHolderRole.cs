namespace Lotus.Roles.Interfaces;

public interface IOverridenTaskHolderRole: ITaskHolderRole
{
    /// <summary>
    /// If this role has common tasks assigned to them
    /// </summary>
    /// <returns>true if this role should have common tasks, otherwise fals</returns>
    public bool AssignCommonTasks();

    /// <summary>
    /// The number of long tasks this role has assigned, if override tasks is true
    /// </summary>
    /// <returns>the number of long tasks to assign</returns>
    public int LongTaskAmount();

    /// <summary>
    /// The number of short tasks this role has assigned, if override tasks is true
    /// </summary>
    /// <returns>the number of short tasks to assign</returns>
    public int ShortTaskAmount();

    /// <summary>
    /// If this role's task count should override the host's default amount
    /// </summary>
    /// <returns>true to override, otherwise false</returns>
    public bool OverrideTasks();
}
namespace Lotus.Roles.Internals.Attributes;

public class ModifiedActionAttribute: RoleActionAttribute
{
    public ModifiedBehaviour Behaviour = ModifiedBehaviour.Replace;

    public ModifiedActionAttribute(RoleActionType actionType, bool triggerAfterDeath = false, bool blockable = false, Priority priority = Priority.NoPriority) : base(actionType, triggerAfterDeath, blockable, priority) { }

    public ModifiedActionAttribute(RoleActionType actionType, ModifiedBehaviour behaviour, bool triggerAfterDeath = false, bool blockable = false,  Priority priority = Priority.NoPriority) : base(actionType, triggerAfterDeath, blockable, priority)
    {
        Behaviour = behaviour;
    }
}

public enum ModifiedBehaviour
{

    /// /// <summary>
    /// Replaces any Role Actions of the same type declared within the class
    /// </summary>
    Replace,

    /// <summary>
    ///
    /// </summary>
    PatchBefore,
    PatchAfter,
    Addition
}
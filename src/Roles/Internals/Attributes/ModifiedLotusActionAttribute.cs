using Lotus.API;
using Lotus.Roles.Internals.Enums;

namespace Lotus.Roles.Internals.Attributes;

public class ModifiedLotusActionAttribute: RoleActionAttribute
{
    public ModifiedBehaviour Behaviour = ModifiedBehaviour.Replace;

    public ModifiedLotusActionAttribute(LotusActionType actionType, bool triggerAfterDeath = false, bool blockable = false, Priority priority = Priority.Normal) : base(actionType, triggerAfterDeath, blockable, priority) { }

    public ModifiedLotusActionAttribute(LotusActionType actionType, ModifiedBehaviour behaviour, bool triggerAfterDeath = false, bool blockable = false,  Priority priority = Priority.Normal) : base(actionType, triggerAfterDeath, blockable, priority)
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
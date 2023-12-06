using System;
using Lotus.Roles2.ComponentRole;

namespace Lotus.Roles2.Attributes.Roles;

public class RoleTriggersAttribute: RoleComponentAttribute
{
    public TriggerBehaviour Behaviour;

    public RoleTriggersAttribute(Type? definition = null, TriggerBehaviour behaviour = TriggerBehaviour.OverrideSpecific) : base(RoleComponentType.RoleTriggers, definition)
    {
        Behaviour = behaviour;
    }
}

public enum TriggerBehaviour
{
    OverrideSpecific,
    OverrideAll,
    NoOverriding
}
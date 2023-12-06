using System;

namespace Lotus.Roles2.ComponentRole;

public class RoleComponentAttribute: Attribute
{
    public RoleComponentType RoleComponentType;
    public Type? Definition;

    public RoleComponentAttribute(RoleComponentType roleComponentType, Type? definition = null)
    {
        this.RoleComponentType = roleComponentType;
        this.Definition = definition;
    }
}
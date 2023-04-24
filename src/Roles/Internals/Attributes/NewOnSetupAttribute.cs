using System;

namespace TOHTOR.Roles.Internals.Attributes;

/// <summary>
/// Automatically calls the constructor OR clone method (if allowed) on the marked field when instancing a role (assigning it to a player).
/// This is SUPER useful (and necessary!) as under normal conditions, the roles assigned to players are SHALLOW copies of each other.
/// Thus this attribute is the easiest way to prevent shallow-copied objets, containing shared data.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class NewOnSetupAttribute : Attribute
{
    public bool UseCloneIfPresent = true;

    public NewOnSetupAttribute(bool useCloneIfPresent)
    {
        UseCloneIfPresent = useCloneIfPresent;
    }

    public NewOnSetupAttribute() : this(true) {}
}
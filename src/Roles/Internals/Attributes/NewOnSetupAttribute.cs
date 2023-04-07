using System;

namespace TOHTOR.Roles.Internals.Attributes;

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
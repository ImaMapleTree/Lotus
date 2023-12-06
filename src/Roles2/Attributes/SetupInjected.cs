using System;

namespace Lotus.Roles2.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
public class SetupInjected: Attribute
{
    public bool UseCloneIfPresent;

    public SetupInjected(bool useCloneIfPresent)
    {
        this.UseCloneIfPresent = useCloneIfPresent;
    }

    public SetupInjected() : this(true) {}

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class Excluded : Attribute
    {
    }
}
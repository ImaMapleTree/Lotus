using System;

namespace Lotus.Roles2.Interfaces;

[AttributeUsage(AttributeTargets.Class)]
public class InstantiateOnSetupAttribute: Attribute
{
    public bool IgnoreInjectionRules;
    public bool IgnorePreSetValues;

    public InstantiateOnSetupAttribute(bool ignoreInjectionRules = false, bool ignorePreSetValues = false)
    {
        IgnoreInjectionRules = ignoreInjectionRules;
        IgnorePreSetValues = ignorePreSetValues;
    }

    public InstantiateOnSetupAttribute(): this(false)
    {
    }
}
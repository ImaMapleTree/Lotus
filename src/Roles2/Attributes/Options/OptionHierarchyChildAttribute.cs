using System;

namespace Lotus.Roles2.Attributes.Options;

[AttributeUsage(AttributeTargets.Field)]
public class OptionHierarchyChildAttribute: Attribute
{
    public string Parent;

    public string? ParentPredicateMethod;
    public PredicateType ParentPredicateType = PredicateType.None;

    public OptionHierarchyChildAttribute(string parent, PredicateType parentPredicateType = PredicateType.None)
    {
        this.Parent = parent;
        this.ParentPredicateType = parentPredicateType;
    }

    public OptionHierarchyChildAttribute(string parent, string parentPredicateMethod)
    {
        this.Parent = parent;
        this.ParentPredicateMethod = parentPredicateMethod;
    }
}
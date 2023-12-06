using System;

namespace Lotus.Roles2.Attributes.Options;

[AttributeUsage(AttributeTargets.Field)]
public class OptionHierarchyParentAttribute: Attribute
{
    internal string? PredicateMethod;
    internal PredicateType SimplePredicateType = PredicateType.None;

    public OptionHierarchyParentAttribute(string predicateMethod)
    {
        this.PredicateMethod = predicateMethod;
    }

    public OptionHierarchyParentAttribute(PredicateType simplePredicateType)
    {
        this.SimplePredicateType = simplePredicateType;
    }
}
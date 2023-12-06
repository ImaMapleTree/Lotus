using System;
using System.Reflection;
using Lotus.GUI;

namespace Lotus.Roles2.Attributes.Options;

public class OptionAttributeRepresentation
{
    public RoleOptionAttribute Attribute { get; }
    public OptionHierarchyChildAttribute? HierarchyChildAttribute { get; }
    public OptionHierarchyParentAttribute? HierarchyParentAttribute { get; }
    public RoleLocalizedAttribute? LocalizedAttribute { get; }
    public InstanceReflector Reflector { get; }

    public OptionAttributeRepresentation(InstanceReflector reflector, RoleOptionAttribute attribute)
    {
        Attribute = attribute;
        HierarchyChildAttribute = reflector.GetAttribute<OptionHierarchyChildAttribute>();
        HierarchyParentAttribute = reflector.GetAttribute<OptionHierarchyParentAttribute>();
        LocalizedAttribute = reflector.GetAttribute<RoleLocalizedAttribute>() ?? attribute.GetType().GetCustomAttribute<RoleLocalizedAttribute>();
        Reflector = reflector;
    }

    public Action<object> CreateBindingFunction()
    {
        if (Reflector.RepresentedType == typeof(Cooldown))
        {
            Cooldown cooldown = (Cooldown)Reflector.GetValue()!;
            return obj => cooldown.SetDuration((float)obj);
        }

        return obj => Reflector.SetValue(obj);
    }
}
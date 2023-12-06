using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles2.Attributes.Roles;
using Lotus.Roles2.ComponentRole;
using Lotus.Roles2.GUI;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles2;

public class DefinitionUnifier
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(DefinitionUnifier));

    private Dictionary<Type, Dictionary<RoleComponentType, List<Type>>> roleComponents = new();

    public UnifiedRoleDefinition Unify(RoleDefinition definition)
    {
        ComponentInstanceManager cim = new(new OrderedSet<Type>(roleComponents.Values.SelectMany(v => v.Values).SelectMany(v => v).ToList()));
        Dictionary<RoleComponentType, List<Type>>? components = roleComponents.GetValueOrDefault(definition.GetType());

        Dictionary<LotusActionType, List<RoleActionStub>> unifiedActionDictionary = new();
        if (components != null)
        {
            unifiedActionDictionary = CreateUnifiedActions(components.GetOrCompute(RoleComponentType.RoleTriggers, () => new List<Type>()), cim);
        }


        return new UnifiedRoleDefinition(definition, cim, unifiedActionDictionary);
    }

    public void RegisterRoleComponents(Assembly assembly)
    {
        foreach (Type flattenAssemblyType in AssemblyUtils.FlattenAssemblyTypes(assembly, AccessFlags.AllAccessFlags))
        {
            bool isRoleDefinition = typeof(RoleDefinition).IsAssignableFrom(flattenAssemblyType);
            if (isRoleDefinition)
            {
                Dictionary<RoleComponentType, List<Type>> components = roleComponents.GetOrCompute(flattenAssemblyType, () => new Dictionary<RoleComponentType, List<Type>>());
                components.GetOrCompute(RoleComponentType.RoleTriggers, () => new List<Type>()).Add(flattenAssemblyType);
                if (typeof(RoleGUI).IsAssignableFrom(flattenAssemblyType))
                    components.GetOrCompute(RoleComponentType.GUI, () => new List<Type>()).Add(flattenAssemblyType);
                continue;
            }

            IEnumerable<RoleComponentAttribute> attributes = flattenAssemblyType.GetCustomAttributes<RoleComponentAttribute>();
            foreach (RoleComponentAttribute attribute in attributes)
            {
                Type? targetDefinition = attribute.Definition;
                if (targetDefinition == null)
                {
                    if (!isRoleDefinition) throw new ArgumentException("RoleComponentAttribute must declare a target-definition type.");
                    targetDefinition = flattenAssemblyType;
                }

                roleComponents.GetOrCompute(targetDefinition, () => new Dictionary<RoleComponentType, List<Type>>())
                    .GetOrCompute(attribute.RoleComponentType, () => new List<Type>()).Add(flattenAssemblyType);
            }
        }
    }

    private Dictionary<LotusActionType, List<RoleActionStub>> CreateUnifiedActions(List<Type> triggerTypes, ComponentInstanceManager cim)
    {
        Dictionary<LotusActionType, List<RoleActionStub>> stubDictionary = new();
        foreach (Type triggerType in triggerTypes.Distinct())
        {
            RoleTriggersAttribute? triggersAttribute = triggerType.GetCustomAttribute<RoleTriggersAttribute>();

            TriggerBehaviour triggerBehaviour = triggersAttribute?.Behaviour ?? TriggerBehaviour.NoOverriding;

            Dictionary<LotusActionType, List<RoleActionStub>> overrideAwareStubs = new();
            foreach ((RoleActionAttribute attr, MethodInfo method) in triggerType.GetMethods(AccessFlags.InstanceAccessFlags).SelectMany(method => method.GetCustomAttributes<RoleActionAttribute>().Select(a => (a, method))))
            {
                RoleActionStub stub = new(attr, method, triggerType);
                List<RoleActionStub> stubs = overrideAwareStubs.GetOrCompute(attr.ActionType, () =>
                {
                    if (triggerBehaviour is not TriggerBehaviour.OverrideAll) return stubDictionary.GetOrCompute(attr.ActionType, () => new List<RoleActionStub>());
                    return stubDictionary[attr.ActionType] = new List<RoleActionStub>();
                });

                IEnumerable<RoleActionOverrideAttribute> overrideAttributes = method.GetCustomAttributes<RoleActionOverrideAttribute>();

                if (triggerBehaviour is TriggerBehaviour.OverrideSpecific)
                {
                    foreach (var roleActionOverrideAttribute in overrideAttributes)
                    {
                        MethodInfo targetMethod = AccessTools.Method(roleActionOverrideAttribute.TargetType, roleActionOverrideAttribute.TargetMethod);
                        if (targetMethod == null) log.Warn($"Could not override method \"{roleActionOverrideAttribute.TargetMethod}\" in class \"{roleActionOverrideAttribute.TargetType}\" - method not found.");
                        else stubs.RemoveAll(s => s.Method == targetMethod);
                    }
                }

                stubs.Add(stub);

            }
        }

        return stubDictionary;
    }
}
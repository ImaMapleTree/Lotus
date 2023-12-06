using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles2.Components;
using Lotus.Roles2.Components.LSI;
using Lotus.Roles2.Interfaces;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles2;

public class GeneratingCIM
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(GeneratingCIM));

    public static readonly Dictionary<Type, Func<IRoleComponent>> ComponentRequirements = new()
    {
        { typeof(OptionConsolidator), () => new OptionConsolidator() },
        { typeof(RelationshipConsolidator), () => new DefaultRelationshipConsolidator() }
    };

    private readonly Dictionary<IRoleComponent, Dictionary<FieldInfo, object?>> fieldInformation = new();
    private readonly Dictionary<IRoleComponent, Dictionary<PropertyInfo, object?>> propertyInformation = new();
    private readonly Dictionary<IRoleComponent, List<InstanceReflector>> reflectors = new();
    private List<IRoleComponent> Instances { get; set; } = null!;
    private Dictionary<Type, IRoleComponent> TypeToObjectRegistry { get; set; } = null!;
    private UnifiedRoleDefinition unifiedRoleDefinition;
    private OrderedSet<Type> componentTypes;

    public GeneratingCIM(UnifiedRoleDefinition unifiedRoleDefinition, OrderedSet<Type> componentTypes)
    {
        this.unifiedRoleDefinition = unifiedRoleDefinition;
        this.componentTypes = componentTypes;
    }

    public Dictionary<FieldInfo, object?> GetFields(IRoleComponent component) => fieldInformation.GetOrCompute(component, () => new Dictionary<FieldInfo, object?>());

    public Dictionary<PropertyInfo, object?> GetProperties(IRoleComponent component) => propertyInformation.GetOrCompute(component, () => new Dictionary<PropertyInfo, object?>());

    public List<InstanceReflector> GetReflectors(IRoleComponent component) => reflectors.GetOrCompute(component, () => new List<InstanceReflector>());

    public T? FindComponent<T>() where T : IRoleComponent
    {
        for (int index = Instances.Count - 1; index >= 0; index--)
        {
            IRoleComponent component = Instances[index];
            if (component is T t) return t;
        }
        return default;
    }

    public IEnumerable<T> FindComponents<T>() where T : IRoleComponent => Instances.OfType<T>();

    internal void GenerateInstances()
    {
        TypeToObjectRegistry = new Dictionary<Type, IRoleComponent>();
        TypeToObjectRegistry[unifiedRoleDefinition.RoleDefinition.GetType()] = unifiedRoleDefinition.RoleDefinition;
        SetupHelper setupHelper = SetupHelper.Reflect(unifiedRoleDefinition.RoleDefinition);
        BindReflectionValues(this, unifiedRoleDefinition.RoleDefinition, setupHelper);

        Instances = componentTypes
            .Where(t => !typeof(RoleDefinition).IsAssignableFrom(t))
            .Select(t =>
            {
                IRoleComponent componentInstance = TypeToObjectRegistry[t] = (IRoleComponent)AccessTools.CreateInstance(t);
                return BindReflectionValues(this, componentInstance, SetupHelper.Reflect(componentInstance));
            })
            .ToList();


        CheckComponentRequirements();

        Instances.Add(unifiedRoleDefinition.RoleDefinition);

        Instances.Concat(fieldInformation.Values.SelectMany(v => v.Values))
            .Concat(propertyInformation.Values.SelectMany(v => v.Values))
            .Where(o => o != null)
            .Distinct()
            .Share(obj =>
            {
                if (obj is IUnifiedDefinitionAware unifiedDefinitionAware) unifiedDefinitionAware.SetUnifiedDefinition(unifiedRoleDefinition);
                if (obj is IDefinitionAware definitionAware) definitionAware.SetRoleDefinition(unifiedRoleDefinition.RoleDefinition);
            }).Share(obj =>
            {
                if (obj is IComponentAware componentAware) componentAware.ReceiveComponents(Instances);
            }).ForEach(obj =>
            {
                if (obj is IPostLinkExecuter postLinkExecuter) postLinkExecuter.PostLinking();
            });
    }

    internal GeneratingCIM CloneAndInstantiate(SetupHelper definitionSetupHelper, UnifiedRoleDefinition instantiatedDefinition)
    {
        GeneratingCIM cloned = CloneUtils.Clone(this);
        cloned.unifiedRoleDefinition = instantiatedDefinition;

        cloned.TypeToObjectRegistry = new Dictionary<Type, IRoleComponent>();
        cloned.TypeToObjectRegistry[instantiatedDefinition.RoleDefinition.GetType()] = instantiatedDefinition.RoleDefinition;

        BindReflectionValues(cloned, instantiatedDefinition.RoleDefinition, definitionSetupHelper);

        cloned.Instances = TypeToObjectRegistry
            .Where(t => !typeof(RoleDefinition).IsAssignableFrom(t.Key))
            .Select(kvp =>
            {
                SetupHelper shHelper = new();
                IRoleComponent component = cloned.TypeToObjectRegistry[kvp.Key] = kvp.Value.Instantiate(shHelper, instantiatedDefinition.RoleDefinition.MyPlayer);
                return BindReflectionValues(cloned, component, shHelper);
            })
            .ToList();

        cloned.Instances.Add(instantiatedDefinition.RoleDefinition);
        cloned.Instances.Concat(cloned.fieldInformation.Values.SelectMany(v => v.Values))
            .Concat(cloned.propertyInformation.Values.SelectMany(v => v.Values))
            .Where(o => o != null)
            .Distinct()
            .Share(obj =>
            {
                if (obj is IUnifiedDefinitionAware unifiedDefinitionAware) unifiedDefinitionAware.SetUnifiedDefinition(cloned.unifiedRoleDefinition);
                if (obj is IDefinitionAware definitionAware) definitionAware.SetRoleDefinition(cloned.unifiedRoleDefinition.RoleDefinition);
            })
            .Share(obj =>
            {
                if (obj is IInstantiatedComponentAware componentAware) componentAware.ReceiveInstantiatedComponents(cloned.Instances);
            }).ForEach(obj =>
            {
              if (obj is IPostInitializationAware postInitializationAware) postInitializationAware.PostInitialization();
            });

        return cloned;
    }

    internal Dictionary<LotusActionType, List<RoleAction>> DefineActions(Dictionary<LotusActionType, List<RoleActionStub>> stubs, Dictionary<LotusActionType, List<RoleAction>> __temporaryDictionary)
    {
        foreach (KeyValuePair<LotusActionType, List<RoleActionStub>> kvp in stubs)
        {
            List<RoleAction> actions = __temporaryDictionary.GetOrCompute(kvp.Key, () => new List<RoleAction>());
            foreach (RoleActionStub roleActionStub in kvp.Value)
            {
                object? instance = TypeToObjectRegistry.GetValueOrDefault(roleActionStub.RequiredExecuter);
                if (instance == null) log.Exception($"Could not collect required executer \"{roleActionStub.RequiredExecuter}\" for RoleAction \"{roleActionStub.Attribute}\". This action will not be run.");
                else actions.Add(roleActionStub.CreateAction(instance));
            }
        }

        return __temporaryDictionary;
    }

    private void CheckComponentRequirements()
    {
        ComponentRequirements.ForEach(kvp =>
        {
            if (TypeToObjectRegistry.ContainsKey(kvp.Key)) return;
            IRoleComponent component = kvp.Value.Invoke();
            TypeToObjectRegistry[kvp.Key] = component;
            Instances.Add(BindReflectionValues(this, component, SetupHelper.Reflect(component)));
        });
    }

    private static IRoleComponent BindReflectionValues(GeneratingCIM cim, IRoleComponent component, SetupHelper setupHelper)
    {
        cim.fieldInformation[component] = setupHelper.Fields;
        cim.propertyInformation[component] = setupHelper.Properties;
        cim.reflectors[component] = setupHelper.Reflectors;
        return component;
    }

}
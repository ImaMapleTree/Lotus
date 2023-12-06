using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Lotus.Roles;
using Lotus.Roles2.Attributes.Options;
using Lotus.Roles2.Interfaces;
using VentLib.Localization;
using VentLib.Options;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles2.Components;

public class OptionConsolidator : IUnifiedDefinitionAware, IComponentAware
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(OptionConsolidator));

    protected UnifiedRoleDefinition UnifiedRoleDefinition { get; private set; } = null!;
    protected List<IRoleOptionModifier> Modifiers { get; } = new();
    protected List<IOptionFilterComponent> Filters { get; } = new();

    private GameOptionBuilder rootBuilder = null!;
    private List<OptionAttributeRepresentation> optionRepresentations = new();

    private GameOption? option;

    public GameOption GetOption()
    {
        if (option == null)
        {
            CreateOptions(rootBuilder, CreateOptionHierarchy(optionRepresentations));
            rootBuilder = Modifiers.Aggregate(rootBuilder, (b, m) => m.ModifyAfter(b));
            option = rootBuilder.BuildAndRegister(OptionManager.GetManager(UnifiedRoleDefinition.Assembly, "roles.txt"));
        }

        return option;
    }

    protected GameOptionBuilder CreateRootOptionBuilder(RoleDefinition definition)
    {
        return new GameOptionBuilder()
            .IOSettings(io => io.UnknownValueAction = ADEAnswer.UseDefault)
            .BindInt(val => definition.Handle.Chance = val)
            .AddIntRange(0, 100, 10, 0, "%")
            .ShowSubOptionPredicate(value => ((int)value) > 0);
    }

    protected GameOptionBuilder DefineOptionBuilder(GameOptionBuilder root, RoleDefinition definition)
    {
        root = root.SubOption(s => s.Name(RoleTranslations.MaximumText)
            .Key("Maximum")
            .AddIntRange(1, 15)
            .Bind(val => definition.Handle.Count = (int)val)
            .ShowSubOptionPredicate(v => 1 < (int)v)
            .SubOption(subsequent => subsequent
                .Name(RoleTranslations.SubsequentChanceText)
                .Key("Subsequent Chance")
                .AddIntRange(10, 100, 10, 0, "%")
                .BindInt(v => definition.Handle.AdditionalChance = v)
                .Build())
            .Build());

        Localizer localizer = Localizer.Get(definition.Assembly);
        string qualifier = $"Roles.{definition.TypeName}.Name";

        return root.LocaleName(qualifier)
            .Tab(definition.OptionTab)
            .Key(definition.TypeName)
            .Color(definition.RoleColor)
            .Description(localizer.Translate($"Roles.{definition.TypeName}.Blurb"))
            .IsHeader(true);
    }

    protected GameOption CreateOptions(GameOptionBuilder root, IEnumerable<OptionRepNode> representations)
    {
        representations.ForEach(rep =>
        {
            OptionAttributeRepresentation attributeRepresentation = rep.Value;
            if (Filters.Any(optionFilterComponent => !optionFilterComponent.PrefilterOption(ref attributeRepresentation))) return;
            GameOptionBuilder intermediate = CreateOption(new GameOptionBuilder(), attributeRepresentation);
            if (Filters.Any(optionFilterComponent => !optionFilterComponent.IntrafilterOption(attributeRepresentation, ref intermediate))) return;
            GameOption result = CreateOptions(intermediate, rep.Children);
            if (Filters.Any(optionFilterComponent => !optionFilterComponent.PostfilterOption(attributeRepresentation, ref result))) return;

            root.SubOption(_ => result);

            OptionHierarchyChildAttribute? hierarchyAttribute = attributeRepresentation.HierarchyChildAttribute;
            if (hierarchyAttribute == null) return;

            if (hierarchyAttribute.ParentPredicateType is PredicateType.FalseValue)
            {
                root.ShowSubOptionPredicate(b => !(bool)b);
                return;
            }

            if (hierarchyAttribute.ParentPredicateType is PredicateType.TrueValue)
            {
                root.ShowSubOptionPredicate(b => (bool)b);
                return;
            }

            object component = attributeRepresentation.Reflector.Instance;
            Type componentType = component.GetType();
            MethodInfo? targetMethod = AccessTools.DeclaredMethod(componentType, hierarchyAttribute.ParentPredicateMethod);
            if (targetMethod == null)
            {
                log.Warn($"Unable to find predicate method \"{hierarchyAttribute.ParentPredicateMethod}\" on component \"{componentType}\" for option \"{root}\"");
                return;
            }

            bool hasParams = targetMethod.GetParameters().Length > 0;
            root.ShowSubOptionPredicate(obj => (bool)targetMethod.Invoke(component, hasParams ? new[] { obj } : Array.Empty<object?>())!);
        });

        return root.Build();
    }

    protected GameOptionBuilder CreateOption(GameOptionBuilder root, OptionAttributeRepresentation representation)
    {
        RoleLocalizedAttribute? localizedAttribute = representation.LocalizedAttribute;
        if (localizedAttribute != null)
        {
            localizedAttribute.Key ??= representation.Reflector.Name;
            localizedAttribute.Group ??= "Options";
        }

        RoleLocalizer localizer = UnifiedRoleDefinition.RoleDefinition.Localizer;
        string optionKey = localizedAttribute?.Translation ?? representation.Reflector.Name;
        string optionName = localizedAttribute == null ? representation.Reflector.Name : localizer.ProvideTranslation(localizedAttribute);
        GameOptionBuilder builder = representation.Attribute.ConfigureBuilder(root.Name(optionName)
            .Key(optionKey)
            .Bind(representation.CreateBindingFunction()), new AttributeContext(localizer, UnifiedRoleDefinition.RoleDefinition, representation, representation.Reflector));

        OptionHierarchyParentAttribute? parentAttribute = representation.HierarchyParentAttribute;
        if (parentAttribute == null) return builder;

        if (parentAttribute.SimplePredicateType is PredicateType.FalseValue) return builder.ShowSubOptionPredicate(b => !(bool)b);
        if (parentAttribute.SimplePredicateType is PredicateType.TrueValue) return builder.ShowSubOptionPredicate(b => (bool)b);

        object component = representation.Reflector.Instance;
        Type componentType = component.GetType();
        MethodInfo? targetMethod = AccessTools.DeclaredMethod(componentType, parentAttribute.PredicateMethod);
        if (targetMethod == null)
        {
            log.Warn($"Unable to find predicate method \"{parentAttribute.PredicateMethod}\" on component \"{componentType}\" for option \"{root}\"");
            return builder;
        }

        bool hasParams = targetMethod.GetParameters().Length > 0;
        return builder.ShowSubOptionPredicate(obj => (bool)targetMethod.Invoke(component, hasParams ? new[] { obj } : Array.Empty<object?>())!);
    }

    protected List<OptionAttributeRepresentation> CreateOptionRepresentations(List<IRoleComponent> components)
    {
        GeneratingCIM generatingCIM = UnifiedRoleDefinition.GetGeneratingCIM();

        return components.SelectMany(c =>
        {
            List<OptionAttributeRepresentation> optionAttributeRepresentations = new();
            if (c is IOptionFilterComponent optionFilter) Filters.Add(optionFilter);
            if (c is IRoleOptionModifier roleOptionBuilder) Modifiers.Add(roleOptionBuilder);
            generatingCIM.GetReflectors(c).ForEach(reflector =>
            {
                RoleOptionAttribute? roleOptionAttribute = reflector.GetAttribute<RoleOptionAttribute>();
                if (roleOptionAttribute == null) return;
                optionAttributeRepresentations.Add(new OptionAttributeRepresentation(reflector, roleOptionAttribute));
            });
            return optionAttributeRepresentations;
        }).ToList();
    }

    public void ProcessReceivedComponents(List<IRoleComponent> components)
    {
        optionRepresentations = CreateOptionRepresentations(components);
        rootBuilder = DefineOptionBuilder(CreateRootOptionBuilder(UnifiedRoleDefinition.RoleDefinition), UnifiedRoleDefinition.RoleDefinition);
        rootBuilder = Modifiers.Aggregate(rootBuilder, (b, initializer) => initializer.ModifyBefore(b));
    }

    public void ReceiveComponents(List<IRoleComponent> components) => ProcessReceivedComponents(components);

    public void SetUnifiedDefinition(UnifiedRoleDefinition definition)
    {
        this.UnifiedRoleDefinition = definition;
    }

    public IRoleComponent Instantiate(SetupHelper setupHelper, PlayerControl player) => this;

    private IEnumerable<OptionRepNode> CreateOptionHierarchy(List<OptionAttributeRepresentation> representations)
    {
        Dictionary<string, OptionRepNode> nodeMap = new();
        Queue<OptionAttributeRepresentation> childNodes = new();

        foreach (OptionAttributeRepresentation optionAttributeRepresentation in representations)
        {
            OptionHierarchyChildAttribute? hierarchy = optionAttributeRepresentation.HierarchyChildAttribute;
            if (hierarchy?.Parent != null) childNodes.Enqueue(optionAttributeRepresentation);
            else nodeMap[optionAttributeRepresentation.Reflector.Name] = new OptionRepNode(optionAttributeRepresentation);
        }

        Dictionary<string, OptionRepNode> parentMap = new(nodeMap);

        int lastSize = 0;
        int size = childNodes.Count;
        while (size != lastSize)
        {
            for (int i = 0; i < size; i++)
            {
                OptionAttributeRepresentation rep = childNodes.Dequeue();
                OptionHierarchyChildAttribute hierarchyChild = rep.HierarchyChildAttribute!;
                if (nodeMap.TryGetValue(hierarchyChild.Parent, out OptionRepNode? parentNode)) parentNode.Children.Add(nodeMap[rep.Reflector.Name] = new OptionRepNode(rep));
                else childNodes.Enqueue(rep);
            }

            lastSize = size;
        }

        return parentMap.Values;
    }

    protected internal class OptionRepNode
    {
        public OptionAttributeRepresentation Value { get; }
        public List<OptionRepNode> Children { get; } = new();

        public OptionRepNode(OptionAttributeRepresentation value)
        {
            Value = value;
        }
    }
}
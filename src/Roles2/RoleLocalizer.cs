using System.Collections.Generic;
using System.Reflection;
using Lotus.Roles2.Interfaces;

namespace Lotus.Roles2;

public abstract class RoleLocalizer: IUnifiedDefinitionAware, IComponentAware
{
    protected UnifiedRoleDefinition Definition { get; private set; } = null!;
    protected Assembly SourceAssembly { get; private set; } = null!;

    public abstract string ProvideTranslation(string qualifier, string? untranslatedText);

    public virtual string ProvideTranslation(RoleLocalizedAttribute roleLocalizedAttribute, string? defaultTranslation = null)
    {
        string qualifier = "Roles." + Definition.RoleDefinition.TypeName;
        if (roleLocalizedAttribute.Group != null)
            qualifier = qualifier + "." + roleLocalizedAttribute.Group;

        qualifier = qualifier + "." + roleLocalizedAttribute.Key;

        return ProvideTranslation(qualifier, roleLocalizedAttribute.Translation ?? defaultTranslation);
    }

    public void SetUnifiedDefinition(UnifiedRoleDefinition unifiedRoleDefinition)
    {
        Definition = unifiedRoleDefinition;
        SourceAssembly = Definition.RoleDefinition.Assembly;
    }

    public void ReceiveComponents(List<IRoleComponent> components)
    {
        ApplyTranslationToComponents(components);
    }

    protected void ApplyTranslationToComponents(List<IRoleComponent> components)
    {
        RoleDefinition roleDefinition = Definition.RoleDefinition;
        GeneratingCIM generatingCIM = Definition.GetGeneratingCIM();
        components.ForEach(c =>
        {
            generatingCIM.GetReflectors(c).ForEach(reflector =>
            {
                if (!reflector.RepresentedType.IsAssignableTo(typeof(string))) return;
                RoleLocalizedAttribute? localizedAttribute = reflector.GetAttribute<RoleLocalizedAttribute>();
                if (localizedAttribute == null) return;
                localizedAttribute.Key ??= reflector.Name;
                string translation = ProvideTranslation(localizedAttribute, reflector.GetValue() as string);
                reflector.SetValue(translation);
            });
        });
    }

    public IRoleComponent Instantiate(SetupHelper setupHelper, PlayerControl player) => this;
}
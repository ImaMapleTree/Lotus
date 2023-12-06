using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.Roles.Internals.Enums;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles2;

public class RoleProperties: IEnumerable<RoleProperty>
{
    public static NamespacedKey<RoleProperties> Key = NamespacedKey.Lotus<RoleProperties>(nameof(RoleProperties));
    private HashSet<RoleProperty> Properties { get; } = new();

    public int Count => Properties.Count;

    public void Add(RoleProperty roleProperty) => Properties.Add(roleProperty);

    public void AddAll(IEnumerable<RoleProperty> roleProperty) => Properties.AddAll(roleProperty);

    public void AddAll(params RoleProperty[] properties) => Properties.AddAll(properties);

    public Func<RoleProperties, RoleProperties> ConcatFunction() => props =>
    {
        this.AddAll(props);
        return this;
    };

    public void Remove(RoleProperty roleProperty) => Properties.Remove(roleProperty);

    public bool HasProperty(RoleProperty roleProperty) => Properties.Contains(roleProperty);

    public bool HasProperty(string name) => Properties.Any(p => p.Name == name);

    public IEnumerator<RoleProperty> GetEnumerator() => Properties.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    // Useful Util Methods

    public static bool IsModifier(UnifiedRoleDefinition roleDefinition) => HasProperty(roleDefinition, RoleProperty.IsModifier);
    public static bool IsAbleToKill(UnifiedRoleDefinition roleDefinition) => HasProperty(roleDefinition, RoleProperty.IsAbleToKill);
    public static bool CannotWinAlone(UnifiedRoleDefinition roleDefinition) => HasProperty(roleDefinition, RoleProperty.CannotWinAlone);
    public static bool IsApparition(UnifiedRoleDefinition roleDefinition) => HasProperty(roleDefinition, RoleProperty.IsApparition);

    public static bool HasProperty(UnifiedRoleDefinition roleDefinition, RoleProperty property) => GetProperties(roleDefinition).Compare(rp => rp.HasProperty(property));
    public static bool IsSpecialType(UnifiedRoleDefinition roleDefinition, SpecialType specialType) => roleDefinition.Metadata.GetOrEmpty(LotusKeys.AuxiliaryRoleType).Compare(rp => rp == specialType);


    public static Optional<RoleProperties> GetProperties(UnifiedRoleDefinition definition) => definition.Metadata.GetOrEmpty(Key);
}
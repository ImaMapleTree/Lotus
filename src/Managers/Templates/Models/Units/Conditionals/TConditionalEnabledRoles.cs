using System.Collections.Generic;
using System.Linq;
using Lotus.Roles;
using Lotus.Roles2;
using Lotus.Roles2.Manager;

namespace Lotus.Managers.Templates.Models.Units.Conditionals;

public class TConditionalEnabledRoles: StringListConditionalUnit
{
    private HashSet<string>? rolesLower;

    public TConditionalEnabledRoles(object input) : base(input)
    {
    }

    public override bool Evaluate(object? data)
    {
        rolesLower ??= Values.Select(r => r.ToLower()).ToHashSet();
        HashSet<string> gameEnabledRoleCache = new();
        bool iterated = false;
        foreach (string role in rolesLower)
        {
            if (gameEnabledRoleCache.Contains(role)) return true;
            if (iterated) continue;
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (UnifiedRoleDefinition roleDefinition in IRoleManager.Current.RoleDefinitions())
            {
                if (!roleDefinition.IsEnabled()) continue;
                gameEnabledRoleCache.Add(roleDefinition.Name.ToLower());
                if (gameEnabledRoleCache.Contains(role)) return true;
            }

            iterated = true;
        }

        return false;
    }
}
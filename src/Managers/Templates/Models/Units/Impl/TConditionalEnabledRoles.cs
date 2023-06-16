using System.Collections.Generic;
using System.Linq;
using Lotus.Roles;

namespace Lotus.Managers.Templates.Models.Units.Impl;

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
            foreach (CustomRole customRole in CustomRoleManager.AllRoles)
            {
                if (!customRole.IsEnabled()) continue;
                gameEnabledRoleCache.Add(customRole.RoleName.ToLower());
                gameEnabledRoleCache.Add(customRole.EnglishRoleName.ToLower());
                if (gameEnabledRoleCache.Contains(role)) return true;
            }

            iterated = true;
        }

        return false;
    }
}
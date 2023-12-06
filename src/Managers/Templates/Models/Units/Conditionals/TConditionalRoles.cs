using System.Collections.Generic;
using System.Linq;
using Lotus.Extensions;
using Lotus.Roles;
using Lotus.Roles2;

namespace Lotus.Managers.Templates.Models.Units.Conditionals;

public class TConditionalRoles: StringListConditionalUnit
{
    private HashSet<string>? rolesLower;

    public TConditionalRoles(object input) : base(input)
    {
    }

    public override bool Evaluate(object? data)
    {
        return data is not PlayerControl player || VerifyRole(player);
    }

    public bool VerifyRole(PlayerControl? player)
    {
        if (player == null) return true;
        rolesLower ??= Values.Select(r => r.ToLower()).ToHashSet();
        return player.GetAllRoleDefinitions().Any(VerifyRole);
    }

    public bool VerifyRole(UnifiedRoleDefinition roleDefinition)
    {
        return rolesLower == null || rolesLower.Contains(roleDefinition.Name);
    }
}
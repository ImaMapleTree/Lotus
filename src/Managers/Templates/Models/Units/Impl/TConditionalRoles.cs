using System.Collections.Generic;
using System.Linq;
using Lotus.Extensions;
using Lotus.Roles;

namespace Lotus.Managers.Templates.Models.Units.Impl;

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
        return VerifyRole(player.GetCustomRole()) || player.GetSubroles().Any(VerifyRole);
    }

    public bool VerifyRole(CustomRole role)
    {
        if (rolesLower == null) return true;
        string englishRoleName = role.EnglishRoleName.ToLower();
        string anyRoleName = role.RoleName.ToLower();
        return rolesLower.Contains(englishRoleName) || rolesLower.Contains(anyRoleName);
    }
}
using System.Collections.Generic;

namespace Lotus.Roles2.Manager;

public class GlobalRoleManager: LotusRoleManager2
{
    public static GlobalRoleManager Instance { get; } = new();

    public override IEnumerable<UnifiedRoleDefinition> RoleDefinitions() => OrderedRoleDefinitions.GetValues();
    internal override bool IsGlobal => true;
}
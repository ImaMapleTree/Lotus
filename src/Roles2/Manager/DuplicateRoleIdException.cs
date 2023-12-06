using System;

namespace Lotus.Roles2.Manager;

public class DuplicateRoleIdException: Exception
{
    public UnifiedRoleDefinition Original { get; }
    public UnifiedRoleDefinition Conflict { get; }

    public DuplicateRoleIdException(object id, UnifiedRoleDefinition original, UnifiedRoleDefinition conflict):
        base($"Duplicate RoleID ({id}) between original=\"{original.GetType().FullName}\" and conflict=\"{conflict.GetType().FullName}\"")
    {
        Original = original;
        Conflict = conflict;
    }
}
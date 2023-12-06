using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lotus.Roles2.Operations;

namespace Lotus.Roles2.Manager;

public interface IRoleManager
{
    public static IRoleManager Current => ProjectLotus.GameModeManager.CurrentGameMode.RoleManager;

    public UnifiedRoleDefinition DefaultDefinition { get; }
    public RoleOperations RoleOperations { get; }

    public IEnumerable<UnifiedRoleDefinition> RoleDefinitions();

    public void RegisterRole(UnifiedRoleDefinition roleDefinition);

    public IEnumerable<UnifiedRoleDefinition> QueryMetadata(Predicate<object> metadataQuery) => RoleDefinitions().Where(rd => rd.Metadata.Select(m => m.Value).Any(obj => metadataQuery(obj)));

    public IEnumerable<UnifiedRoleDefinition> QueryMetadata(string key, object value) => RoleDefinitions().Where(rd => rd.Metadata.Any(k => k.Key.Key == key && k.Value == value));

    public IEnumerable<UnifiedRoleDefinition> QueryMetadata(object value) => QueryMetadata(md => md == value);

    public UnifiedRoleDefinition GetRole(ulong assemblyRoleID, Assembly? assembly = null);

    public UnifiedRoleDefinition GetRole(string globalRoleID);

    public UnifiedRoleDefinition GetRole(Type roleType) => RoleDefinitions().FirstOrDefault(d => d.RoleDefinition.GetType() == roleType)!;

    public UnifiedRoleDefinition GetRole<T>() where T: RoleDefinition => RoleDefinitions().FirstOrDefault(d => d.RoleDefinition.GetType() == typeof(T))!;
}
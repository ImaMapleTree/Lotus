using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lotus.Roles2.Definitions;
using Lotus.Roles2.Operations;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles2.Manager;

public class LotusRoleManager2: IRoleManager
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(LotusRoleManager2));
    private readonly Dictionary<Assembly, Dictionary<ulong, UnifiedRoleDefinition>> definitionsByAssembly = new();

    public UnifiedRoleDefinition DefaultDefinition => NoOpDefinition.Instance;
    public RoleOperations RoleOperations { get; } = new StandardRoleOperations();

    public virtual IEnumerable<UnifiedRoleDefinition> RoleDefinitions() => OrderedRoleDefinitions.GetValues().Concat(GlobalRoleManager.Instance.RoleDefinitions());
    protected OrderedDictionary<string, UnifiedRoleDefinition> OrderedRoleDefinitions { get; } = new();

    internal virtual bool IsGlobal => false;

    public virtual void RegisterRole(UnifiedRoleDefinition roleDefinition)
    {
        if (!IsGlobal && roleDefinition.GlobalRoleID.StartsWith("G"))
        {
            GlobalRoleManager.Instance.RegisterRole(roleDefinition);
            return;
        }
        ulong roleID = roleDefinition.RoleID;

        log.Debug($"Registering Role Definition (name={roleDefinition.Name}, RoleID={roleDefinition.RoleID}, Assembly={roleDefinition.Assembly.GetName().Name}, AddonID={roleDefinition.Addon?.UUID ?? 0})");

        Dictionary<ulong, UnifiedRoleDefinition> assemblyDefinitions = definitionsByAssembly.GetOrCompute(roleDefinition.Assembly, () => new Dictionary<ulong, UnifiedRoleDefinition>());
        if (!assemblyDefinitions.TryAdd(roleID, roleDefinition)) throw new DuplicateRoleIdException(roleID, assemblyDefinitions[roleID], roleDefinition);
        if (!OrderedRoleDefinitions.TryAdd(roleDefinition.GlobalRoleID, roleDefinition)) throw new DuplicateRoleIdException(roleDefinition.GlobalRoleID, OrderedRoleDefinitions[roleDefinition.GlobalRoleID], roleDefinition);
    }

    public virtual UnifiedRoleDefinition GetRole(ulong assemblyRoleID, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        if (definitionsByAssembly[assembly].TryGetValue(assemblyRoleID, out UnifiedRoleDefinition? role)) return role;
        throw new NoSuchRoleException($"Could not find role with ID \"{assemblyRoleID}\" from roles defined by: \"{assembly.FullName}\"");
    }

    public virtual UnifiedRoleDefinition GetRole(string globalRoleID)
    {
        if (!IsGlobal && globalRoleID.StartsWith("G")) return GlobalRoleManager.Instance.GetRole(globalRoleID);
        if (OrderedRoleDefinitions.TryGetValue(globalRoleID, out UnifiedRoleDefinition? role)) return role;
        throw new NoSuchRoleException($"Could not find role with global-ID \"{globalRoleID}\"");
    }
}
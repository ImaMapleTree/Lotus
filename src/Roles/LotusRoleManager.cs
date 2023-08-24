using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Lotus.Roles.Builtins;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Enums;
using VentLib.Options;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles;

public class LotusRoleManager
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(LotusRoleManager));

    public OptionManager RoleOptionManager = OptionManager.GetManager(file: "role_options.txt");
    public List<CustomRole> AllRoles = new();

    public EmptyRole Default = null!;
    public VanillaRoles Vanilla = null!;
    public InternalRoles Internal = null!;

    private readonly OrderedDictionary<LotusRoleType, List<CustomRole>> roleTypeDictionary = new();
    private Dictionary<ulong, LotusRoleType> lotusRoleTypes = new();
    private Dictionary<Type, RemoteList<IRoleInitializer>> roleInitializers = new();

    private bool frozen;

    public IEnumerable<CustomRole> Not(params LotusRoleType[] roleType)
    {
        return roleType.Aggregate<LotusRoleType, IEnumerable<KeyValuePair<LotusRoleType, List<CustomRole>>>>(roleTypeDictionary,
            (current, rt) => current.Where(kvp => !kvp.Key.Equals(rt))
            ).SelectMany(kvp => kvp.Value);
    }

    public IEnumerable<CustomRole> All(params LotusRoleType[] roleType)
    {
        return roleType.Aggregate<LotusRoleType, IEnumerable<KeyValuePair<LotusRoleType, List<CustomRole>>>>(roleTypeDictionary,
            (current, rt) => current.Where(kvp => kvp.Key.Equals(rt))
        ).SelectMany(kvp => kvp.Value);
    }

    public void AddRole(CustomRole role, LotusRoleType roleType)
    {
        role.LotusRoleType = roleType;
        AddLotusRoleType(roleType);
        roleTypeDictionary.GetOrCompute(roleType, () => new List<CustomRole>()).Add(role);
    }

    internal void AddRole(CustomRole role) => AddRole(role, role.LotusRoleType);

    public Remote<IRoleInitializer> AddRoleInitializers(Type type, IRoleInitializer roleInitializer)
    {
        log.Info($"Adding Role Initializer for {type}");
        return roleInitializers.GetOrCompute(type, () => new RemoteList<IRoleInitializer>()).Add(roleInitializer);
    }

    public RemoteList<IRoleInitializer> GetInitializersForType(Type type) => roleInitializers.GetOrCompute(type, () => new RemoteList<IRoleInitializer>());

    public CustomRole GetCleanRole(CustomRole role) => GetRole(GetIdentifier(role));

    public string GetIdentifier(CustomRole role)
    {
        return $"{role.LotusRoleType.TypeID}.{role.DeclaringAssembly.GetName().Name?.Replace(".", "_") ?? "Unknown"}.{role.GetType().Name.Replace(".", "_")}.{role.EnglishRoleName}";
    }

    public CustomRole GetRole(string identifier)
    {
        string[] splits = identifier.Split(".");
        ulong roleTypeID = ulong.Parse(splits[0]);
        string assemblyName = splits[1];
        string typeName = splits[2];
        string englishRoleName = splits[3];

        if (!lotusRoleTypes.TryGetValue(roleTypeID, out LotusRoleType roleType))
            throw new DataException($"Could not find {nameof(LotusRoleType)} for TypeID={roleTypeID}.");

        if (!roleTypeDictionary.TryGetValue(roleType, out List<CustomRole> matchedRoles))
            throw new DataException($"Could not get roles for {nameof(LotusRoleType)}(Name={roleType.Name}, TypeID={roleTypeID})");

        return matchedRoles.FirstOrDefault(r =>
            (r.DeclaringAssembly.GetName().Name?.Replace(".", "_") ?? "Unknown") == assemblyName
            && r.GetType().Name.Replace(".", "_") == typeName
            && r.EnglishRoleName == englishRoleName, Default);
    }

    public CustomRole GetRoleFromName(string name)
    {
        return AllRoles.FirstOrOptional(r => r.RoleName == name)
            .CoalesceEmpty(() => ProjectLotus.RoleManager.AllRoles.FirstOrOptional(r => r.EnglishRoleName == name))
            .OrElse(ProjectLotus.RoleManager.Default);
    }

    internal void Load()
    {
        Default = new EmptyRole();
        Vanilla = new VanillaRoles();
        Internal = new InternalRoles();
    }

    internal void Freeze()
    {
        if (frozen) return;
        AllRoles.AddRange(roleTypeDictionary.GetValues().SelectMany(r => r));
        AllRoles.ForEach(r => r.Solidify());
        frozen = true;
    }

    private void AddLotusRoleType(LotusRoleType roleType)
    {
        if (!lotusRoleTypes.TryGetValue(roleType.TypeID, out LotusRoleType existingType) || existingType.Name == roleType.Name)
        {
            lotusRoleTypes[roleType.TypeID] = roleType;
            return;
        }

        throw new ArgumentException(
            $"{nameof(LotusRoleType)} TypeID conflict! Types have same TypeID ({existingType.TypeID}) but different names! ({existingType.Name} != {roleType.Name}). " +
            $"Either make the names match, or pick a different TypeID!", nameof(roleType));
    }

    public class VanillaRoles
    {
        public Impostor Impostor = new Impostor();
        public Shapeshifter Shapeshifter = new Shapeshifter();
        public Crewmate Crewmate = new Crewmate();
        public Engineer Engineer = new Engineer();
        public Scientist Scientist = new Scientist();
    }

    public class InternalRoles
    {
        public IllegalRole IllegalRole = new IllegalRole();
        public GameMaster GameMaster = new GameMaster();
    }
}
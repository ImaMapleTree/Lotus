using System.Collections.Generic;
using System.Reflection;
using Lotus.Factions.Interfaces;
using Lotus.Roles;
using Lotus.Extensions;
using Lotus.Gamemodes;
using Lotus.Roles.Internals.Enums;
using VentLib.Utilities.Extensions;
using Version = VentLib.Version.Version;

namespace Lotus.Addons;

public abstract class LotusAddon
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(LotusAddon));

    internal List<CustomRole>? ExportedRoles;
    internal readonly List<IFaction> Factions = new();

    internal readonly Assembly BundledAssembly = Assembly.GetCallingAssembly();
    internal readonly ulong Uuid;

    public abstract string Name { get; }
    public abstract Version Version { get; }

    public LotusAddon()
    {
        Uuid = (BundledAssembly.GetIdentity(false)?.SemiConsistentHash() ?? 0ul + Name.SemiConsistentHash());
    }

    internal string GetName(bool fullName = false) => !fullName
        ? Name
        : $"{BundledAssembly.FullName}::{Name}-{Version.ToSimpleName()}";

    public abstract void Initialize();

    public virtual void PostInitialize(List<LotusAddon> addons)
    {
    }

    public void ExportRoles(IEnumerable<CustomRole> roles, LotusRoleType roleType)
    {
        roles.ForEach(r =>
        {
            log.Trace($"Exporting Role: {r.EnglishRoleName}", "ExportRoles");
            ProjectLotus.RoleManager.AddRole(r, roleType);
        });
    }

    public void ExportGamemodes(IEnumerable<IGamemode> gamemodes)
    {
        foreach (IGamemode gamemode in gamemodes)
        {
            log.Trace($"Exporting Gamemode: {gamemode.Name}", "ExportGamemodes");
            ProjectLotus.GamemodeManager.Gamemodes.Add(gamemode);
        }
    }

    public void ExportGamemodes(params IGamemode[] gamemodes) => ExportGamemodes((IEnumerable<IGamemode>)gamemodes);

    public override string ToString() => GetName(true);
}


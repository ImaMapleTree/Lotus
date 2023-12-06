using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lotus.Factions.Interfaces;
using Lotus.Roles;
using Lotus.Extensions;
using Lotus.GameModes;
using Lotus.GameModes.Standard;
using Lotus.Roles2;
using VentLib.Utilities.Extensions;
using Version = VentLib.Version.Version;

namespace Lotus.Addons;

public abstract class LotusAddon
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(LotusAddon));

    internal Dictionary<RoleDefinition, HashSet<IGameMode>> ExportedDefinitions { get; } = new();

    internal readonly List<IFaction> Factions = new();

    internal readonly Assembly BundledAssembly = Assembly.GetCallingAssembly();
    internal readonly ulong UUID;

    public abstract string Name { get; }
    public abstract Version Version { get; }

    public LotusAddon()
    {
        UUID = (ulong)HashCode.Combine(BundledAssembly.GetIdentity(false)?.SemiConsistentHash() ?? 0ul, Name.SemiConsistentHash());
    }

    internal string GetName(bool fullName = false) => !fullName
        ? Name
        : $"{BundledAssembly.FullName}::{Name}-{Version.ToSimpleName()}";

    public abstract void Initialize();

    public virtual void PostInitialize(List<LotusAddon> addons)
    {
    }

    public void ExportRoleDefinitions(IEnumerable<RoleDefinition> roleDefinitions, params Type[] baseGameModes)
    {
        if (baseGameModes.Length == 0) ExportRoleDefinitions(roleDefinitions, StandardGameMode.Instance);
        else ExportRoleDefinitions(roleDefinitions, baseGameModes.Select(gm => ProjectLotus.GameModeManager.GetGameMode(gm) ?? StandardGameMode.Instance).ToArray());
    }

    public void ExportRoleDefinitions(IEnumerable<RoleDefinition> roleDefinitions, params IGameMode[] baseGameModes)
    {
        IGameMode[] targetGameModes = ProjectLotus.GameModeManager.GameModes.Where(gm => baseGameModes.Any(bgm => bgm.GetType().IsInstanceOfType(gm))).ToArray();
        roleDefinitions.ForEach(r =>
        {
            r.Addon = this;
            ExportedDefinitions.GetOrCompute(r, () => new HashSet<IGameMode>()).AddAll(targetGameModes);
        });
    }

    public void ExportGameModes(IEnumerable<IGameMode> gamemodes)
    {
        foreach (IGameMode gamemode in gamemodes)
        {
            log.Trace($"Exporting GameMode: {gamemode.Name}", "ExportGameModes");
            ProjectLotus.GameModeManager.GameModes.Add(gamemode);
        }
    }

    public void ExportGameModes(params IGameMode[] gamemodes) => ExportGameModes((IEnumerable<IGameMode>)gamemodes);

    public override string ToString() => GetName(true);
}


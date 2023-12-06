using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Lotus.Roles2.Interfaces;
using Lotus.Roles2.Manager;
using VentLib.Utilities.Collections;

namespace Lotus.Roles2.Operations;

// ReSharper disable once InconsistentNaming
public interface RoleOperations: IRoleComponent
{
    public static RoleOperations Current => IRoleManager.Current.RoleOperations;

    public void Assign(UnifiedRoleDefinition role, PlayerControl player);

    public Relation Relationship(UnifiedRoleDefinition source, UnifiedRoleDefinition comparison);

    public Relation Relationship(UnifiedRoleDefinition source, IFaction comparison);

    public Relation Relationship(PlayerControl source, PlayerControl comparison) => Relationship(source.PrimaryRole(), comparison.PrimaryRole());

    public void SyncOptions(PlayerControl target) => SyncOptions(target, Game.MatchData.Roles.GetRoleDefinitions(target.PlayerId));

    public void SyncOptions(UnifiedRoleDefinition definition) => SyncOptions(definition.MyPlayer, definition);

    public void SyncOptions(PlayerControl target, params UnifiedRoleDefinition[] definitions) => SyncOptions(target, definitions, null);

    public void SyncOptions(PlayerControl target, IEnumerable<UnifiedRoleDefinition> definitions, IEnumerable<GameOptionOverride>? overrides = null, bool deepSet = false);

    public ActionHandle Trigger(LotusActionType action, PlayerControl? source, ActionHandle handle, params object[] parameters);

    public ActionHandle Trigger(LotusActionType action, PlayerControl? source, params object[] parameters) => Trigger(action, source, ActionHandle.NoInit(), parameters);

    public ActionHandle TriggerFor(IEnumerable<UnifiedRoleDefinition> recipients, LotusActionType action, PlayerControl? source, ActionHandle handle, params object[] parameters);

    public ActionHandle TriggerFor(IEnumerable<UnifiedRoleDefinition> recipients, LotusActionType action, PlayerControl? source, params object[] parameters) => TriggerFor(recipients, action, source, ActionHandle.NoInit(), parameters);

    public ActionHandle TriggerFor(IEnumerable<PlayerControl> players, LotusActionType action, PlayerControl? source, ActionHandle handle, params object[] parameters) => TriggerFor(players.SelectMany(p => p.GetAllRoleDefinitions()), action, source, handle, parameters);

    public ActionHandle TriggerFor(IEnumerable<PlayerControl> players, LotusActionType action, PlayerControl? source, params object[] parameters) => TriggerFor(players, action, source, ActionHandle.NoInit(), parameters);

    public ActionHandle TriggerFor(PlayerControl player, LotusActionType action, PlayerControl? source, params object[] parameters) => TriggerFor(new Singleton<PlayerControl>(player), action, source, parameters);

    public ActionHandle TriggerFor(PlayerControl player, LotusActionType action, PlayerControl? source, ActionHandle handle, params object[] parameters) => TriggerFor(new Singleton<PlayerControl>(player), action, source, handle, parameters);

    public ActionHandle TriggerForAll(LotusActionType action, PlayerControl? source, ActionHandle handle, params object[] parameters) => TriggerFor(Players.GetPlayers(), action, source, handle, parameters);

    public ActionHandle TriggerForAll(LotusActionType action, PlayerControl? source,  params object[] parameters) => TriggerFor(Players.GetPlayers(), action, source, parameters);
}
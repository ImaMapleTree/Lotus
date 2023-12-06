using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Options;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Lotus.Roles2.Interfaces;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles2.Operations;

public class StandardRoleOperations: RoleOperations
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(StandardRoleOperations));
    protected virtual RoleAssigner RoleAssigner { get; } = new StandardRoleAssigner();

    public void Assign(UnifiedRoleDefinition role, PlayerControl player)
    {
        RoleAssigner.Assign(role, player);
        if (player.AmOwner) role.RoleDefinition.GUIProvider.Start();
    }

    public Relation Relationship(UnifiedRoleDefinition source, UnifiedRoleDefinition comparison) => source.RelationshipConsolidator.Relationship(comparison);

    public Relation Relationship(UnifiedRoleDefinition source, IFaction comparison) => source.RelationshipConsolidator.Relationship(comparison);

    public void SyncOptions(PlayerControl target, IEnumerable<UnifiedRoleDefinition> definitions, IEnumerable<GameOptionOverride>? overrides = null, bool deepSet = false)
    {
        if (target == null || !AmongUsClient.Instance.AmHost) return;

        overrides = CalculateOverrides(target, definitions, overrides);

        IGameOptions modifiedOptions = DesyncOptions.GetModifiedOptions(overrides);
        if (deepSet) RpcV3.Immediate(PlayerControl.LocalPlayer.NetId, RpcCalls.SyncSettings).Write(modifiedOptions).Send(target.GetClientId());
        DesyncOptions.SyncToPlayer(modifiedOptions, target);
    }

    public ActionHandle Trigger(LotusActionType action, PlayerControl? source, ActionHandle handle, params object[] parameters)
    {
        if (action is not LotusActionType.FixedUpdate)
        {
            Game.CurrentGameMode.Trigger(action, handle, parameters);
            if (handle.Cancellation is not (ActionHandle.CancelType.Soft or ActionHandle.CancelType.None)) return handle;
        }

        return TriggerFor(Players.GetPlayers().SelectMany(p => p.GetAllRoleDefinitions()), action, source, handle, parameters);
    }

    public ActionHandle TriggerFor(IEnumerable<UnifiedRoleDefinition> recipients, LotusActionType action, PlayerControl? source, ActionHandle handle, params object[] parameters)
    {
        if (action is LotusActionType.FixedUpdate)
        {
            foreach (UnifiedRoleDefinition unifiedRoleDefinition in recipients)
            {
                if (unifiedRoleDefinition.Actions.TryGetValue(action, out List<RoleAction>? actions) && !actions.IsEmpty())
                    actions[0].ExecuteFixed();
            }
            return handle;
        }

        parameters = parameters.AddToArray(handle);
        object[] globalActionParameters = parameters;
        if (!ReferenceEquals(source, null))
        {
            globalActionParameters = new object[parameters.Length + 1];
            Array.Copy(parameters, 0, globalActionParameters, 1, parameters.Length);
            globalActionParameters[0] = source;
        }

        IEnumerable<(RoleAction, UnifiedRoleDefinition)> actionsAndDefinitions = recipients.SelectMany(r => (r.GetActions(action).Select(a => (a, r)))).OrderBy(t => t.a.Priority);
        foreach ((RoleAction roleAction, UnifiedRoleDefinition roleDefinition) in actionsAndDefinitions)
        {
            PlayerControl myPlayer = roleDefinition.MyPlayer;
            if (handle.Cancellation is not (ActionHandle.CancelType.None or ActionHandle.CancelType.Soft)) continue;
            if (myPlayer == null) continue;
            if (!roleAction.CanExecute(myPlayer, source)) continue;

            try
            {
                if (roleAction.ActionType.IsPlayerAction())
                {
                    Hooks.PlayerHooks.PlayerActionHook.Propagate(new PlayerActionHookEvent(myPlayer, roleAction, parameters));
                    Trigger(LotusActionType.PlayerAction, myPlayer, handle, roleAction, parameters);
                }

                handle.ActionType = action;

                if (handle.Cancellation is not (ActionHandle.CancelType.None or ActionHandle.CancelType.Soft)) continue;

                roleAction.Execute(roleAction.Flags.HasFlag(ActionFlag.GlobalDetector) ? globalActionParameters : parameters);
            }
            catch (Exception e)
            {
                log.Exception($"Failed to execute RoleAction {action}.", e);
            }
        }

        return handle;
    }

    protected IEnumerable<GameOptionOverride> CalculateOverrides(PlayerControl player, IEnumerable<UnifiedRoleDefinition> definitions, IEnumerable<GameOptionOverride>? overrides)
    {
        IEnumerable<GameOptionOverride> definitionOverrides = definitions.SelectMany(d => d.UnifiedRoleOverrides);
        definitionOverrides = definitionOverrides.Concat(Game.MatchData.Roles.GetOverrides(player.PlayerId));
        if (overrides != null) definitionOverrides = definitionOverrides.Concat(overrides);
        return definitionOverrides;
    }

    public IRoleComponent Instantiate(SetupHelper setupHelper, PlayerControl player) => this;
}
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Factions.Impostors;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Logging;
using Lotus.Roles.Overrides;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles2.Operations;

public class StandardRoleAssigner: RoleAssigner
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(StandardRoleAssigner));

    public void Assign(UnifiedRoleDefinition definition, PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        bool isStartOfGame = Game.State is GameState.InIntro or GameState.InLobby;

        PlayerControl[] alliedPlayers = Players.GetPlayers().Where(p => RoleOperations.Current.Relationship(player, p) is Relation.FullAllies).ToArray();

        RoleDefinition roleDefinition = definition.RoleDefinition;
        RoleTypes vanillaRole = roleDefinition.Role;

        if (vanillaRole.IsCrewmate())
        {
            DevLogger.Log($"Real Role: {vanillaRole}");
            player.RpcSetRole(vanillaRole);

            if (!isStartOfGame) goto finishAssignment;

            Players.GetPlayers().ForEach(p => p.GetTeamInfo().AddPlayer(player.PlayerId, player.GetVanillaRole().IsImpostor()));

            goto finishAssignment;
        }


        log.Trace($"Setting {player.name} Role => {vanillaRole} | IsStartGame = {isStartOfGame}", "CustomRole::Assign");
        if (player.IsHost()) player.SetRole(vanillaRole);
        else RpcV3.Immediate(player.NetId, RpcCalls.SetRole).Write((ushort)vanillaRole).Send(player.GetClientId());

        log.Debug($"Player {player.GetNameWithRole()} Allies: [{alliedPlayers.Select(p => p.name).Fuse()}]");
        HashSet<byte> alliedPlayerIds = alliedPlayers.Where(p => roleDefinition.Faction.CanSeeRole(p)).Select(p => p.PlayerId).ToHashSet();
        int[] alliedPlayerClientIds = alliedPlayers.Where(p => roleDefinition.Faction.CanSeeRole(p)).Select(p => p.GetClientId()).ToArray();

        PlayerControl[] crewmates = Players.GetPlayers().Where(p =>
        {
            DevLogger.Log($"Checking: {p.name} ({p.GetVanillaRole()})");
            return p.GetVanillaRole().IsCrewmate();
        }).ToArray();
        int[] crewmateClientIds = crewmates.Select(p => p.GetClientId()).ToArray();
        log.Trace($"Current Crewmates: [{crewmates.Select(p => p.name).Fuse()}]");

        PlayerControl[] nonAlliedImpostors = Players.GetPlayers().Where(p => p.GetVanillaRole().IsImpostor()).Where(p => !alliedPlayerIds.Contains(p.PlayerId) && p.PlayerId != player.PlayerId).ToArray();
        int[] nonAlliedImpostorClientIds = nonAlliedImpostors.Select(p => p.GetClientId()).ToArray();
        log.Trace($"Non Allied Impostors: [{nonAlliedImpostors.Select(p => p.name).Fuse()}]");

        RpcV3.Immediate(player.NetId, RpcCalls.SetRole).Write((ushort)vanillaRole).SendInclusive(alliedPlayerClientIds);
        if (isStartOfGame) alliedPlayers.ForEach(p => p.GetTeamInfo().AddPlayer(player.PlayerId, vanillaRole.IsImpostor()));

        RpcV3.Immediate(player.NetId, RpcCalls.SetRole).Write((ushort)RoleTypes.Crewmate).SendInclusive(nonAlliedImpostorClientIds);
        if (isStartOfGame) nonAlliedImpostors.ForEach(p => p.GetTeamInfo().AddVanillaCrewmate(player.PlayerId));

        // This code exists to hopefully better split up the roles to cause less blackscreens
        RoleTypes splitRole = roleDefinition.Faction is ImpostorFaction ? RoleTypes.Impostor : RoleTypes.Crewmate;
        RpcV3.Immediate(player.NetId, RpcCalls.SetRole).Write((ushort)splitRole).SendInclusive(crewmateClientIds);
        if (isStartOfGame) crewmates.ForEach(p =>
        {
            if (splitRole is RoleTypes.Impostor) p.GetTeamInfo().AddVanillaImpostor(player.PlayerId);
            else p.GetTeamInfo().AddVanillaCrewmate(player.PlayerId);
        });

        finishAssignment:

        ShowRoleToTeammates(player, definition, alliedPlayers);

        // This is for host
        if (RoleOperations.Current.Relationship(player, PlayerControl.LocalPlayer) is Relation.FullAllies && roleDefinition.Faction.CanSeeRole(PlayerControl.LocalPlayer)) player.SetRole(vanillaRole);
        else player.SetRole(PlayerControl.LocalPlayer.GetVanillaRole().IsImpostor() ? RoleTypes.Crewmate : RoleTypes.Impostor);

        RoleOperations.Current.SyncOptions(player, player.GetAllRoleDefinitions(), new GameOptionOverride[] { new(Override.KillCooldown, 0.1f)} , true);
    }

    private void ShowRoleToTeammates(PlayerControl player, UnifiedRoleDefinition roleDefinition, IEnumerable<PlayerControl> allies)
    {
        // Currently only impostors can show each other their roles
        RoleHolder roleHolder = player.NameModel().GetComponentHolder<RoleHolder>();
        if (roleHolder.Count == 0)
        {
            log.Warn("Error Showing Roles to Allies. Role Component does not exist.", "CustomRole::ShowRoleToTeammates");
            return;
        }
        RoleComponent roleComponent = roleHolder[0];
        allies.Where(roleDefinition.Faction.CanSeeRole).ForEach(a =>
        {
            log.Trace($"Showing Role {roleDefinition.Name} to {a.name}", "ShowRoleToTeammates");
            roleComponent.AddViewer(a);
        });
    }

}
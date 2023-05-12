using System.Collections.Generic;
using System.Linq;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Managers;
using TOHTOR.Roles;
using TOHTOR.Roles.Subroles;
using TOHTOR.RPC;
using VentLib.Networking.RPC.Attributes;

namespace TOHTOR.API;

public partial class Api
{
    public class Roles
    {
        public static List<CustomRole> GetEnabledRoles() => CustomRoleManager.AllRoles.Where(r => r.IsEnabled()).ToList();

        [ModRPC((uint) ModCalls.SetCustomRole, RpcActors.Host, RpcActors.NonHosts, MethodInvocation.ExecuteBefore)]
        public static void AssignRole(PlayerControl player, CustomRole role, bool sendToClient = false)
        {
            CustomRole assigned = Game.MatchData.Roles.MainRoles[player.PlayerId] = role.Instantiate(player);
            if (Game.State is GameState.InLobby or GameState.InIntro) player.GetTeamInfo().MyRole = role.RealRole;
            if (sendToClient) assigned.Assign();
        }

        [ModRPC((uint) ModCalls.SetSubrole, RpcActors.Host, RpcActors.NonHosts, MethodInvocation.ExecuteBefore)]
        public static void AssignSubrole(PlayerControl player, CustomRole role, bool sendToClient = false)
        {
            CustomRole instantiated = role.Instantiate(player);
            Game.MatchData.Roles.AddSubrole(player.PlayerId, instantiated);
            if (sendToClient) role.Assign();
        }
    }
}
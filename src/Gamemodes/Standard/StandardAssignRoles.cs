using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Managers;
using TOHTOR.Options;
using TOHTOR.Roles;
using TOHTOR.Roles.Interfaces;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Roles.Subroles;
using TOHTOR.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Gamemodes.Standard;

class StandardAssignRoles
{
    internal static void StandardAssign(List<PlayerControl> unassignedPlayers)
    {
        // Only constant
        int impostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;

        Queue<CustomRole> roles = new();
        RoleDistributor.DistributeImpostors(roles, impostors);
        RoleDistributor.DistributeNonImpostors(roles, unassignedPlayers.Count);

        List<CustomRole> allAssignedRoles = new();


        int i = 0;
        while (GeneralOptions.DebugOptions.NameBasedRoleAssignment && i < unassignedPlayers.Count)
        {
            PlayerControl player = unassignedPlayers[i];
            CustomRole? role = CustomRoleManager.AllRoles.FirstOrDefault(r => r.RoleName.RemoveHtmlTags().ToLower().StartsWith(player.UnalteredName()?.ToLower() ?? "HEHXD"));
            if (role != null && role.GetType() != typeof(Crewmate))
            {
                Game.AssignRole(player, role);
                allAssignedRoles.Add(role);
                unassignedPlayers.Pop(i);
            }
            else i++;
        }

        unassignedPlayers = unassignedPlayers.Sorted(p => p.IsHost() ? 1 : 0).ToList();
        while (unassignedPlayers.Count > 0 && roles.Count > 0)
        {
            PlayerControl assignedPlayer = unassignedPlayers.PopRandom();
            CustomRole role = roles.Dequeue();
            // We have to initialize the role past its "static" phase
            Game.AssignRole(assignedPlayer, IVariableRole.PickAssignedRole(role));
            allAssignedRoles.Add(role);
        }

        while (unassignedPlayers.Count > 0)
        {
            PlayerControl unassigned = unassignedPlayers.Pop();
            CustomRole crewmate = CustomRoleManager.Default;
            Game.AssignRole(unassigned, crewmate);
            allAssignedRoles.Add(crewmate);
        }

        List<Subrole> subroles = CustomRoleManager.AllRoles.OfType<Subrole>().ToList();
        while (subroles.Count > 0)
        {
            Subrole subrole = subroles.PopRandom();
            bool hasSubrole = subrole.Chance > UnityEngine.Random.RandomRange(0, 100);
            if (!hasSubrole) continue;

            subrole = IVariableSubrole.PickAssignedRole(subrole);

            List<PlayerControl> victims = Game.GetAllPlayers().Where(p => p.GetSubrole() == null).ToList();
            if (victims.Count == 0) break;
            PlayerControl victim = victims.GetRandom();
            CustomRoleManager.AddPlayerSubrole(victim.PlayerId, (Subrole)subrole.Instantiate(victim));
            allAssignedRoles.Add(subrole);
        }

        Game.GetAllPlayers().Sorted(p => p.IsHost() ? 0 : 1).ForEach(p => p.GetCustomRole().Assign());
    }
}
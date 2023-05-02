extern alias JBAnnotations;
using System.Collections.Generic;
using System.Linq;
using JBAnnotations::JetBrains.Annotations;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Gamemodes.Standard.Lotteries;
using TOHTOR.Managers;
using TOHTOR.Options;
using TOHTOR.Roles;
using TOHTOR.Roles.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Roles.Subroles;
using TOHTOR.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Gamemodes.Standard;

public class StandardRoleAssignmentLogic
{
    private static List<IAdditionalAssignmentLogic> _additionalAssignmentLogics = new();

    [UsedImplicitly]
    public static void AddAdditionalAssignmentLogic(IAdditionalAssignmentLogic logic) => _additionalAssignmentLogics.Add(logic);

    public static void AssignRoles(List<PlayerControl> allPlayers)
    {
        List<PlayerControl> unassignedPlayers = new(allPlayers);
        RoleDistribution distribution = GeneralOptions.GameplayOptions.OptimizeRoleAssignment
            ? OptimizeRoleAlgorithm.OptimizeDistribution()
            : OptimizeRoleAlgorithm.NonOptimizedDistribution();


        int j = 0;
        while (GeneralOptions.DebugOptions.NameBasedRoleAssignment && j < unassignedPlayers.Count)
        {
            PlayerControl player = unassignedPlayers[j];
            CustomRole? role = CustomRoleManager.AllRoles.FirstOrDefault(r => r.RoleName.RemoveHtmlTags().ToLower().StartsWith(player.name?.ToLower() ?? "HEHXD"));
            if (role != null && role.GetType() != typeof(Crewmate))
            {
                Game.AssignRole(player, role);
                unassignedPlayers.Pop(j);
            }
            else j++;
        }


        // ASSIGN NK ROLES
        NeutralKillingLottery nkLottery = new();
        for (int i = 0; i < distribution.NeutralKillingSlots && unassignedPlayers.Count > 0; i++)
        {
            CustomRole role = nkLottery.Next();
            if (role is IllegalRole)
            {
                if (distribution.OpenFlexSlot++ < distribution.FlexImpostorSlots) distribution.Impostors++;
                continue;
            }
            Game.AssignRole(unassignedPlayers.PopRandom(), IVariableRole.PickAssignedRole(role));
        }
        // ====================

        // ASSIGN IMPOSTOR ROLES
        ImpostorLottery impostorLottery = new();
        for (int i = 0; i < distribution.Impostors && unassignedPlayers.Count > 0; i++)
            Game.AssignRole(unassignedPlayers.PopRandom(), IVariableRole.PickAssignedRole(impostorLottery.Next()));
        // =====================

        // ASSIGN NEUTRAL ROLES
        NeutralLottery neutralLottery = new();
        for (int i = 0; i < distribution.NeutralPassiveSlots && unassignedPlayers.Count > 0; i++)
        {
            CustomRole role = neutralLottery.Next();
            if (role is IllegalRole) continue;
            Game.AssignRole(unassignedPlayers.PopRandom(), IVariableRole.PickAssignedRole(role));
        }
        // ===================

        // DO ADDITIONAL LOGIC (ADDONS)
        foreach (IAdditionalAssignmentLogic additionalAssignmentLogic in _additionalAssignmentLogics)
            additionalAssignmentLogic.AssignRoles(allPlayers, unassignedPlayers);
        // ===================

        // ASSIGN CREWMATE ROLES
        CrewmateLottery crewmateLottery = new();
        while (unassignedPlayers.Count > 0) Game.AssignRole(unassignedPlayers.PopRandom(), crewmateLottery.Next());
        // ====================

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
        }

        Game.GetAllPlayers().Sorted(p => p.IsHost() ? 0 : 1).ForEach(p => p.GetCustomRole().Assign());
    }

}
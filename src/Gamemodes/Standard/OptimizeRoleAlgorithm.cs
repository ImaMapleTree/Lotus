using TOHTOR.Options;
using UnityEngine;

namespace TOHTOR.Gamemodes.Standard;

public class OptimizeRoleAlgorithm
{
    public static RoleDistribution OptimizeDistribution()
    {
        int impostorsMax = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;

        int npSlots = RoleOptions.NeutralOptions.NeutralPassiveSlots;
        int nkSlots = RoleOptions.NeutralOptions.NeutralKillingSlots;

        int totalPlayers = PlayerControl.AllPlayerControls.Count;

        // Absolute max number of killing roles
        int maxKilling = Mathf.CeilToInt(totalPlayers / 2f) - 1;

        int totalKillingRoles = 1;

        int adjustedNkSlots = 0;
        int impostors = 1;

        while (totalKillingRoles < maxKilling)
        {
            totalKillingRoles++;
            if (nkSlots-- > 0) adjustedNkSlots++;
            else if (impostorsMax-- > 0) impostors++;
            else break;
        }

        if (impostorsMax + adjustedNkSlots + npSlots >= totalPlayers) npSlots--;

        return new RoleDistribution
        {
            Impostors = impostors,
            NeutralKillingSlots = adjustedNkSlots,
            NeutralPassiveSlots = npSlots,
            FlexImpostorSlots = impostorsMax,
        };
    }

    public static RoleDistribution NonOptimizedDistribution()
    {
        return new RoleDistribution
        {
            Impostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors,
            NeutralKillingSlots = RoleOptions.NeutralOptions.NeutralKillingSlots,
            NeutralPassiveSlots = RoleOptions.NeutralOptions.NeutralPassiveSlots,
            FlexImpostorSlots = 0
        };
    }
}
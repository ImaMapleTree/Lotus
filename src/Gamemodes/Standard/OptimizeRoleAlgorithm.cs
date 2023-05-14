using Il2CppSystem;
using Lotus.Options;
using UnityEngine;

namespace Lotus.Gamemodes.Standard;

public class OptimizeRoleAlgorithm
{
    public static RoleDistribution OptimizeDistribution()
    {
        int impostorsMax = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
        int totalPlayers = PlayerControl.AllPlayerControls.Count;
        int impostorCount = totalPlayers switch
        {
            <= 6 => 1,
            <= 11 => 2,
            _ => 3
        };
        impostorCount = Math.Min(impostorCount, impostorsMax);


        return new RoleDistribution
        {
            Impostors = impostorCount,
            MinimumNeutralPassive = RoleOptions.NeutralOptions.MinimumNeutralPassiveRoles,
            MaximumNeutralPassive = RoleOptions.NeutralOptions.MaximumNeutralPassiveRoles,
            MinimumNeutralKilling = RoleOptions.NeutralOptions.MinimumNeutralKillingRoles,
            MaximumNeutralKilling = RoleOptions.NeutralOptions.MaximumNeutralKillingRoles
        };
    }

    public static RoleDistribution NonOptimizedDistribution()
    {
        return new RoleDistribution
        {
            Impostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors,
            MinimumNeutralPassive = RoleOptions.NeutralOptions.MinimumNeutralPassiveRoles,
            MaximumNeutralPassive = RoleOptions.NeutralOptions.MaximumNeutralPassiveRoles,
            MinimumNeutralKilling = RoleOptions.NeutralOptions.MinimumNeutralKillingRoles,
            MaximumNeutralKilling = RoleOptions.NeutralOptions.MaximumNeutralKillingRoles
        };
    }
}
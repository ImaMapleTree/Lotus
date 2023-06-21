using System.Collections.Generic;
using System.Linq;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.RoleGroups.Crew.Alchemist.Ingredients;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Crew.Alchemist;

public partial class Alchemist
{
    private bool helpedFixLights;

    protected override void OnTaskComplete(Optional<NormalPlayerTask> _)
    {
        LocalIngredients.Add(new IngredientCatalyst());
    }

    private static void CheckChaosSpawn()
    {
        if (Random.RandomRangeInt(0, IngredientChaos.SpawnOdds + 1) != IngredientChaos.SpawnOdds) return;
        VentLogger.Trace("Spawning Chaos Ingredient");
        GlobalIngredients.Add(new IngredientChaos());
    }

    private static void CheckDiscussionSpawn()
    {
        if (Random.RandomRangeInt(0, IngredientTinkering.SpawnOdds + 1) != IngredientTinkering.SpawnOdds) return;
        List<PlayerControl> alivePlayers = Players.GetPlayers(PlayerFilter.Alive).ToList();
        if (alivePlayers.Count < 3) return;
        PlayerControl randomAlivePlayer = alivePlayers.GetRandom();
        if (randomAlivePlayer.GetPlayersInAbilityRangeSorted().Count >= 2)
            GlobalIngredients.Add(new IngredientTinkering(randomAlivePlayer.GetTruePosition()));
    }

    [RoleAction(RoleActionType.AnyDeath)]
    private void PlayerDiesEssence(PlayerControl death, PlayerControl killer)
    {
        if (killer.PlayerId == MyPlayer.PlayerId) return;
        DeadBody? body = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(db => db.ParentId == death.PlayerId);
        if (body == null) return;
        GlobalIngredients.Add(new IngredientDeath(body));
    }

    [RoleAction(RoleActionType.SabotagePartialFix)]
    private void AssistedWithLights(PlayerControl fixer)
    {
        helpedFixLights = helpedFixLights || fixer.PlayerId == MyPlayer.PlayerId;
    }

    [RoleAction(RoleActionType.SabotageFixed)]
    private void CheckSightSpawn()
    {
        if (helpedFixLights) LocalIngredients.Add(new IngredientSight());
        helpedFixLights = false;
    }

    [RoleAction(RoleActionType.AnyShapeshift)]
    private void ShapeshiftSpawn(PlayerControl shapeshifter)
    {
        GlobalIngredients.Add(new IngredientPurity(shapeshifter.GetTruePosition()));
    }
}
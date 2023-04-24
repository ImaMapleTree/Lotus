using System.Collections.Generic;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.RoleGroups.Crew.Ingredients;
using UnityEngine;
using VentLib.Localization.Attributes;

namespace TOHTOR.Roles.RoleGroups.Crew.Potions;

public class PotionKilling: Potion
{
    [Localized("Death")]
    public static string PotionName = "Potion of Death";

    public PotionKilling(int requiredCatalyst) : base((1, Ingredient.Death), (requiredCatalyst, Ingredient.Catalyst))
    {
    }

    public override string Name() => PotionName;

    public override Color Color() => FactionInstances.TheUndead.FactionColor();

    public override bool Use(PlayerControl user)
    {
        List<PlayerControl> sortedPlayers = user.GetPlayersInAbilityRangeSorted();
        if (sortedPlayers.Count == 0) return false;
        user.InteractWith(sortedPlayers[0], DirectInteraction.FatalInteraction.Create(user.GetCustomRole()));
        return true;
    }
}
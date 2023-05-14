using Lotus.Roles.RoleGroups.Crew.Ingredients;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.Crew.Potions;

public class PotionVoting: Potion
{
    [Localized("Voting")]
    private static string PotionName = "Leader Potion";

    public PotionVoting(int requiredCatalyst) : base((1, Ingredient.Tinkering), (requiredCatalyst, Ingredient.Catalyst))
    {
    }

    public override string Name() => PotionName;

    public override Color Color() => UnityEngine.Color.green;

    public override bool Use(PlayerControl user)
    {
        user.GetCustomRole<Alchemist>().ExtraVotes += 1;
        return true;
    }
}
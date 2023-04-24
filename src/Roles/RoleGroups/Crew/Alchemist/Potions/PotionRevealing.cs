using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Roles.RoleGroups.Crew.Ingredients;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Crew.Potions;

public class PotionRevealing: Potion
{
    [Localized("Revealing")]
    public static string PotionName = "Serene Grace";
    private string gradientName = TextUtils.ApplyGradient(PotionName, new Color(1f, 0.93f, 0.98f), new Color(1f, 0.57f, 0.73f));

    public PotionRevealing(int requiredCatalyst) : base((2, Ingredient.Shifting), (requiredCatalyst, Ingredient.Catalyst))
    {
    }

    public override string Name() => gradientName;

    public override Color Color() => new(1f, 0.57f, 0.73f);

    public override bool Use(PlayerControl user)
    {
        List<PlayerControl> players = user.GetPlayersInAbilityRangeSorted();
        if (players.Count == 0) return false;
        PlayerControl random = players.GetRandom();
        random.NameModel().GetComponentHolder<RoleHolder>()[^1].AddViewer(user);
        return true;
    }
}
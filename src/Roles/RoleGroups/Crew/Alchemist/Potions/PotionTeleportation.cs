using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Roles.RoleGroups.Crew.Ingredients;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Crew.Potions;

public class PotionTeleportation: Potion
{
    [Localized("Teleportation")]
    public static string PotionName = "Warp Potion";

    public PotionTeleportation(int requiredCatalyst) : base((1, Ingredient.Chaos), (requiredCatalyst, Ingredient.Catalyst))
    {
    }

    public override string Name() => PotionName;

    public override Color Color() => new(1f, 0.22f, 0.61f);

    public override bool Use(PlayerControl user)
    {
        List<PlayerControl> players = Game.GetAlivePlayers().Where(p => p.PlayerId != user.PlayerId).ToList();
        if (players.Count == 0) return true;
        PlayerControl randomPlayer = players.GetRandom();
        Vector2 myPosition = user.GetTruePosition();
        Vector2 otherPosition = randomPlayer.GetTruePosition();

        Utils.Teleport(user.NetTransform, otherPosition);
        Utils.Teleport(randomPlayer.NetTransform, myPosition);
        return true;
    }
}
using Lotus.Roles.RoleGroups.Crew.Alchemist.Ingredients.Internal;
using UnityEngine;
using VentLib.Localization.Attributes;
using Random = UnityEngine.Random;

namespace Lotus.Roles.RoleGroups.Crew.Alchemist.Ingredients;

public class IngredientChaos: Ingredient
{
    [Localized("Chaos")]
    public static string IngredientName = "Theta Rift";

    public const int SpawnOdds = 3600; // Actually 1/3600

    public IngredientChaos() : base(Random.Range(1, 15))
    {
    }

    public override string Name() => "Theta Rift";

    public override Color Color() => new(0.82f, 0.35f, 0.08f);

    public override string Symbol() => "Θ";

    public override bool IsCollectable(Alchemist collector) => true;
}
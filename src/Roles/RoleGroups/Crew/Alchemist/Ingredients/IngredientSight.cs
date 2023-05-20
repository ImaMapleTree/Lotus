using UnityEngine;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.Crew.Ingredients;

public class IngredientSight: Ingredient
{
    [Localized("Sight")]
    public static string IngredientName = "Essence of Sight";

    public IngredientSight() : base(5f)
    {
    }

    public override string Name() => IngredientName;

    public override Color Color() => new(0.55f, 0.16f, 0.74f);

    public override string Symbol() => "☀";

    public override bool IsCollectable(Alchemist collector) => true;
}
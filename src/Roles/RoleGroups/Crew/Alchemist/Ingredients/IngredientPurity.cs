using System.Linq;
using UnityEngine;
using VentLib.Localization.Attributes;

namespace Lotus.Roles.RoleGroups.Crew.Ingredients;

public class IngredientPurity: Ingredient, IWorldIngredient
{
    private Vector2 position;

    [Localized("Purity")]
    public static string IngredientName = "Shifting Rose";

    public IngredientPurity(Vector2 position) : base(12f)
    {
        this.position = position;
    }

    public override string Name() => IngredientName;

    public override Color Color() => new(1f, 0.62f, 0.91f);

    public override string Symbol() => "❀";

    public override bool IsCollectable(Alchemist collector) => RoleUtils.GetPlayersWithinDistance(position, CollectRadius()).Any(p => p.PlayerId == collector.MyPlayer.PlayerId);

    public Vector2 Position() => position;

    public float CollectRadius() => 2f;
}
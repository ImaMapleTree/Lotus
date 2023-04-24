using System.Linq;
using UnityEngine;
using VentLib.Localization.Attributes;

namespace TOHTOR.Roles.RoleGroups.Crew.Ingredients;

public class IngredientTinkering: Ingredient, IWorldIngredient
{
    [Localized("Tinkering")]
    public static string IngredientName = "Fragment of Discussions";

    public const int SpawnOdds = 500; // Actually 1/500

    private Vector2 position;

    public IngredientTinkering(Vector2 position) : base(10f)
    {
        this.position = position;
    }

    public override string Name() => IngredientName;

    public override Color Color() => new(0.45f, 1f, 0.46f);

    public override string Symbol() => "◯";

    public override bool IsCollectable(Alchemist collector) => RoleUtils.GetPlayersWithinDistance(position, CollectRadius()).Any(p => p.PlayerId == collector.MyPlayer.PlayerId);

    public Vector2 Position() => position;

    public float CollectRadius() => 0.8f;
}
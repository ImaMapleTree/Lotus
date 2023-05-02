using System.Linq;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Factions;
using UnityEngine;
using VentLib.Localization.Attributes;

namespace TOHTOR.Roles.RoleGroups.Crew.Ingredients;

public class IngredientDeath: Ingredient, IWorldIngredient
{
    [Localized("Death")]
    public static string IngredientName = "Vial of Decay";

    private DeadBody? deadBody;
    private Vector2 position;

    public IngredientDeath(DeadBody? deadBody) : base(20f)
    {
        this.deadBody = deadBody!;
        this.position = this.deadBody != null ? deadBody!.TruePosition : default;
    }

    public override string Name() => "Vial of Decay";

    public override Color Color() => FactionInstances.TheUndead.FactionColor();

    public override string Symbol() => "⚠";

    public override bool IsCollectable(Alchemist collector) => RoleUtils.GetPlayersWithinDistance(position, CollectRadius()).Any(p => p.PlayerId == collector.MyPlayer.PlayerId);

    public Vector2 Position() => position;

    public float CollectRadius() => 2f;

    public override void Collect()
    {
        if (deadBody != null) Game.GameStates.UnreportableBodies.Add(deadBody.ParentId);
    }
}
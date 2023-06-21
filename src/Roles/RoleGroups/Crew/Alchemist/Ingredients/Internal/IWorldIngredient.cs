using UnityEngine;

namespace Lotus.Roles.RoleGroups.Crew.Alchemist.Ingredients.Internal;

public interface IWorldIngredient : IAlchemyIngredient
{
    public Vector2 Position();

    public float CollectRadius();
}
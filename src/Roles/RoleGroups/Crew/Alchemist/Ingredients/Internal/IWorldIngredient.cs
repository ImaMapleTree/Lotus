using UnityEngine;

namespace Lotus.Roles.RoleGroups.Crew.Ingredients;

public interface IWorldIngredient : IAlchemyIngredient
{
    public Vector2 Position();

    public float CollectRadius();
}
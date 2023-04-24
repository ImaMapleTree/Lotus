using UnityEngine;

namespace TOHTOR.Roles.RoleGroups.Crew.Ingredients;

public interface IAlchemyIngredient
{
    public string Name();

    public Color Color();

    public string Symbol();

    public bool IsCollectable(Alchemist collector);

    public bool IsExpired();

    public IngredientInfo AsInfo();

    public void Collect();
}
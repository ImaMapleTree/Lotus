using System;
using UnityEngine;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Crew.Ingredients;

public class IngredientInfo
{
    public readonly Type Type;
    public readonly Color Color;
    public readonly string Symbol;

    public IngredientInfo(IAlchemyIngredient ingredient)
    {
        Type = ingredient.GetType();
        Color = ingredient.Color();
        Symbol = ingredient.Symbol();
    }

    public override int GetHashCode() => Type.GetHashCode();

    public override bool Equals(object? obj) => (obj is IngredientInfo info) && info.Type == Type;

    public override string ToString() => Color.Colorize(Symbol);
}
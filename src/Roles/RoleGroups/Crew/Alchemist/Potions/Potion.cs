using TOHTOR.Roles.RoleGroups.Crew.Ingredients;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Crew.Potions;

[Localized("Roles.Alchemist.Potions")]
public abstract class Potion : ICraftable
{
    protected OrderedDictionary<IngredientInfo, int> Recipe = new();

    protected Potion(params (int amount, IngredientInfo ingredient)[] ingredients)
    {
        ingredients.ForEach(i =>
        {
            if (Recipe.Contains(i.ingredient)) Recipe[i.ingredient] += i.amount;
            else Recipe.Add(i.ingredient, i.amount);
        });
    }

    public abstract string Name();

    public abstract Color Color();

    public OrderedDictionary<IngredientInfo, int> Ingredients() => Recipe;

    public abstract bool Use(PlayerControl user);
}
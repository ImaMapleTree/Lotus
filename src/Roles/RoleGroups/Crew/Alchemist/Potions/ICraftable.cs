using TOHTOR.Roles.RoleGroups.Crew.Ingredients;
using UnityEngine;
using VentLib.Utilities.Collections;

namespace TOHTOR.Roles.RoleGroups.Crew.Potions;

public interface ICraftable
{
    public string Name();

    public Color Color();

    public OrderedDictionary<IngredientInfo, int> Ingredients();

    public bool Use(PlayerControl user);
}